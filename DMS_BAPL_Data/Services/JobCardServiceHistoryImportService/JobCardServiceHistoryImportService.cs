using DMS_BAPL_Data.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.JobCardServiceHistoryImportService
{
    /// <summary>
    /// Imports "Jobcard servicehistory.xlsx" (DMS_ServiceHistory export, 63 columns, ~4.8 lakh rows, ~140 MB)
    /// into BAPLDMSvad.dbo.JobCardServiceHistory (the DMS database, same as the rest of the API).
    ///
    /// Rules
    ///  * Only dbo.JobCardServiceHistory is written. JobCardHeader / dbo.JobCards and the Job Card screens are not touched.
    ///  * All 63 columns are kept (Excel "Id" → SourceId). The literal text "NULL" (how the export writes empty
    ///    values) is stored as SQL NULL. Nothing is truncated: a value too long / not a number / not a date fails the row.
    ///  * UniqueKey identifies a job. Duplicates inside the file: first row wins, the rest are Skipped.
    ///    UniqueKeys already in the table: Skipped - existing rows are never updated or overwritten.
    ///  * Rows are bulk-copied to a #temp table and inserted with INSERT … WHERE NOT EXISTS, one transaction per batch.
    ///    A failed batch rolls back alone; re-running the import is safe (committed rows are skipped).
    ///  * Validate (DryRun) runs the same code path but never writes (only SELECTs against the table, if it exists).
    ///  * Validate / import run as background jobs (the Azure App Service front end cuts requests at ~230 s);
    ///    one job at a time per API instance + sp_getapplock in the database for real imports.
    ///
    /// Uses the API's existing "DefaultConnection" (BAPLDMSvad) - no separate connection string.
    /// </summary>
    public sealed class JobCardServiceHistoryImportService : IJobCardServiceHistoryImportService
    {
        public const string ConnectionName = "DefaultConnection";
        public const string TableName = "dbo.JobCardServiceHistory";
        private const string AppLock = "DMS_JobCardServiceHistoryImport";
        private const int MaxIssuesPerLevel = 1000;
        private static readonly string[] AuditColumns = { "UniqueKey", "ImportBatchId", "ImportedBy", "SourceFileName", "SourceRowNo" };
        private static readonly TimeSpan UploadTtl = TimeSpan.FromHours(12);
        private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "dms-jcsh-imports");

        private readonly IConfiguration _config;
        private readonly ILogger<JobCardServiceHistoryImportService> _logger;

        private sealed record UploadInfo(Guid Id, string Path, string FileName, long Size, string SheetName, int LastRow,
                                         List<string> Headers, string UserId, DateTime UploadedAt);

        private sealed class Job
        {
            public readonly object Sync = new();
            public JcshJobStatus Status = null!;
            public CancellationTokenSource Cts = new();
            public Stopwatch Clock = Stopwatch.StartNew();
            public readonly Dictionary<string, int> LevelShown = new();
            public readonly Dictionary<string, JcshDealerSummary> Dealers = new(StringComparer.OrdinalIgnoreCase);
        }

        private readonly ConcurrentDictionary<Guid, UploadInfo> _uploads = new();
        private readonly ConcurrentDictionary<Guid, Job> _jobs = new();
        private readonly object _runLock = new();
        private Job? _running;

        public JobCardServiceHistoryImportService(IConfiguration config, ILogger<JobCardServiceHistoryImportService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private string? ConnectionString => _config.GetConnectionString(ConnectionName);

        // ═════════════════════════════════════════════════════════════════════
        // FIELDS
        // ═════════════════════════════════════════════════════════════════════
        public List<JcshFieldViewModel> GetFields() => JcshColumns.All.Select(c => new JcshFieldViewModel
        {
            Key = c.Key,
            Label = c.Key == "Id" ? "Id (source)" : c.Key,
            Group = c.Group,
            Required = c.Required,
            Target = $"{c.Db} {c.SqlType}"
        }).ToList();

        private static string Norm(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        // ═════════════════════════════════════════════════════════════════════
        // UPLOAD
        // ═════════════════════════════════════════════════════════════════════
        public async Task<JcshUploadResponse> UploadAsync(Stream excel, string fileName, string userId, CancellationToken ct)
        {
            CleanupOldUploads();
            Directory.CreateDirectory(TempDir);
            var id = Guid.NewGuid();
            var path = Path.Combine(TempDir, id.ToString("N") + ".xlsx");
            await using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 20, useAsync: true))
                await excel.CopyToAsync(fs, ct);

            try
            {
                var response = new JcshUploadResponse { UploadId = id, FileName = fileName, FileSize = new FileInfo(path).Length, Fields = GetFields() };
                int lastRow;
                using (var fs = File.OpenRead(path))
                using (var wb = new LargeExcelWorkbook(fs))
                {
                    response.Sheets = wb.Worksheets.Select(s => new JcshSheetInfo { Name = s.Name, Rows = Math.Max(0, s.CountRowsFast() - 1) }).ToList();
                    // the sheet whose header row has the most of the expected columns (normally the only sheet)
                    LargeExcelSheet? best = null; List<string> bestHeaders = new(); var bestScore = -1;
                    var expected = JcshColumns.All.Select(c => Norm(c.Key)).ToHashSet();
                    foreach (var s in wb.Worksheets)
                    {
                        var hdr = ReadHeaders(s.PeekRow(1));
                        var score = hdr.Count(h => expected.Contains(Norm(h)));
                        if (score > bestScore) { best = s; bestHeaders = hdr; bestScore = score; }
                    }
                    if (best == null || bestScore <= 0)
                        throw new ArgumentException("No sheet has the Jobcard servicehistory columns in row 1 (DealerCode, JobNo, JobDate, … UniqueKey).");

                    response.SheetName = best.Name;
                    response.Headers = bestHeaders;
                    lastRow = best.CountRowsFast();
                    response.TotalRows = Math.Max(0, lastRow - 1);

                    var byNorm = bestHeaders.GroupBy(Norm).ToDictionary(g => g.Key, g => g.First());
                    foreach (var c in JcshColumns.All)
                        if (byNorm.TryGetValue(Norm(c.Key), out var h)) response.SuggestedMapping[c.Key] = h;
                    response.MissingColumns = JcshColumns.All.Where(c => !response.SuggestedMapping.ContainsKey(c.Key)).Select(c => c.Key).ToList();
                    var used = response.SuggestedMapping.Values.ToHashSet();
                    response.ExtraColumns = bestHeaders.Where(h => !used.Contains(h)).ToList();

                    foreach (var row in best.StreamRows(2).Take(5))
                    {
                        var sample = new Dictionary<string, string?>();
                        for (var i = 0; i < bestHeaders.Count; i++)
                        {
                            var v = row.Cell(i + 1).Value;
                            sample[bestHeaders[i]] = v.IsBlank ? null : v.IsDateTime ? v.GetDateTime().ToString("dd.MM.yyyy HH:mm") : v.ToString(CultureInfo.InvariantCulture);
                        }
                        response.SampleRows.Add(sample);
                    }
                }

                (response.DestinationConfigured, response.DestinationTableExists, response.DestinationMessage) = await CheckDestinationAsync(ct);
                _uploads[id] = new UploadInfo(id, path, fileName, response.FileSize, response.SheetName, lastRow, response.Headers, userId, DateTime.UtcNow);
                return response;
            }
            catch
            {
                TryDelete(path);
                throw;
            }
        }

        private static List<string> ReadHeaders(LargeExcelRow row)
        {
            var list = new List<string>();
            var last = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (var c = 1; c <= last; c++)
            {
                var v = row.Cell(c).Value;
                list.Add(v.IsBlank ? $"Column{c}" : v.ToString(CultureInfo.InvariantCulture).Trim());
            }
            return list;
        }

        private async Task<(bool configured, bool exists, string? message)> CheckDestinationAsync(CancellationToken ct)
        {
            var cs = ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                return (false, false, $"Connection string '{ConnectionName}' is not configured on the API. Validation runs without the database duplicate check; import is disabled.");
            try
            {
                var b = new SqlConnectionStringBuilder(cs) { ConnectTimeout = 15 };
                await using var con = new SqlConnection(b.ConnectionString);
                await con.OpenAsync(ct);
                // table must exist AND have every column this import writes (guards against an unrelated table of the same name)
                await using var cmd = new SqlCommand($"SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'{TableName}', N'U')", con) { CommandTimeout = 15 };
                var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                await using (var rd = await cmd.ExecuteReaderAsync(ct))
                    while (await rd.ReadAsync(ct)) found.Add(rd.GetString(0));
                if (found.Count == 0)
                    return (true, false, $"Connected to {b.InitialCatalog}, but {TableName} does not exist yet - run 01_Create_JobCardServiceHistory.sql (after approval). Import is disabled until then.");
                var missing = JcshColumns.All.Select(c => c.Db).Concat(AuditColumns).Where(n => !found.Contains(n)).ToList();
                if (missing.Count > 0)
                    return (true, false, $"{TableName} exists in {b.InitialCatalog} but is missing column(s): {string.Join(", ", missing)}. Import is disabled - check the table against 01_Create_JobCardServiceHistory.sql.");
                return (true, true, $"Connected to {b.InitialCatalog}; {TableName} found ({found.Count} columns).");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Service history destination check failed");
                return (true, false, "Could not connect to the DMS database: " + ex.GetBaseException().Message);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // JOBS
        // ═════════════════════════════════════════════════════════════════════
        public JcshJobStatus Start(Guid uploadId, JcshRunOptions options, string userId)
        {
            if (!_uploads.TryGetValue(uploadId, out var up) || !File.Exists(up.Path))
                throw new KeyNotFoundException("The uploaded file is no longer on the server (expired or API restarted). Add the file again.");

            options.BatchSize = Math.Clamp(options.BatchSize <= 0 ? 5000 : options.BatchSize, 500, 20000);
            options.Mapping ??= new Dictionary<string, string>();
            var missing = JcshColumns.All.Where(c => c.Required && !options.Mapping.ContainsKey(c.Key)).Select(c => c.Key).ToList();
            if (missing.Count > 0) throw new ArgumentException("Map the required column(s): " + string.Join(", ", missing));
            foreach (var kv in options.Mapping)
            {
                if (!JcshColumns.ByKey.ContainsKey(kv.Key)) throw new ArgumentException($"Unknown field '{kv.Key}'.");
                if (!up.Headers.Contains(kv.Value)) throw new ArgumentException($"Column '{kv.Value}' is not in the file.");
            }
            var dupHeader = options.Mapping.GroupBy(kv => kv.Value).FirstOrDefault(g => g.Count() > 1);
            if (dupHeader != null) throw new ArgumentException($"Column '{dupHeader.Key}' is mapped to more than one field.");

            var from = Math.Max(2, options.FromRow ?? 2);
            var to = Math.Min(up.LastRow, options.ToRow is > 0 ? options.ToRow.Value : up.LastRow);
            if (to < from) throw new ArgumentException("Row range is empty.");

            var job = new Job();
            job.Status = new JcshJobStatus
            {
                JobId = Guid.NewGuid(),
                UploadId = uploadId,
                DryRun = options.DryRun,
                FileName = up.FileName,
                SheetName = up.SheetName,
                StartedAt = DateTime.UtcNow,
                TotalRows = to - from + 1,
                State = "Queued",
                Phase = "Queued"
            };

            lock (_runLock)
            {
                if (_running != null && (_running.Status.State is "Queued" or "Running"))
                    throw new InvalidOperationException($"A service history {(_running.Status.DryRun ? "validation" : "import")} is already running ({_running.Status.FileName}). Wait for it to finish.");
                _running = job;
            }
            _jobs[job.Status.JobId] = job;
            foreach (var old in _jobs.Where(j => j.Value.Status.FinishedAt < DateTime.UtcNow.AddHours(-12)).Select(j => j.Key).ToList())
                _jobs.TryRemove(old, out _);

            _ = Task.Run(() => RunAsync(job, up, options, from, to, userId));
            return Snapshot(job);
        }

        public JcshJobStatus? GetJob(Guid jobId) => _jobs.TryGetValue(jobId, out var j) ? Snapshot(j) : null;

        public JcshJobStatus? Cancel(Guid jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var j)) return null;
            if (j.Status.State is "Queued" or "Running") { j.Cts.Cancel(); lock (j.Sync) j.Status.Phase = "Stopping…"; }
            return Snapshot(j);
        }

        public bool Discard(Guid uploadId, string userId)
        {
            if (_running != null && _running.Status.UploadId == uploadId && _running.Status.State is "Queued" or "Running") return false;
            if (!_uploads.TryRemove(uploadId, out var up)) return false;
            TryDelete(up.Path);
            return true;
        }

        private static JcshJobStatus Snapshot(Job j)
        {
            lock (j.Sync)
            {
                var s = j.Status;
                return new JcshJobStatus
                {
                    JobId = s.JobId, UploadId = s.UploadId, DryRun = s.DryRun, State = s.State, Phase = s.Phase,
                    FileName = s.FileName, SheetName = s.SheetName, StartedAt = s.StartedAt, FinishedAt = s.FinishedAt,
                    ElapsedSeconds = Math.Round(j.Clock.Elapsed.TotalSeconds, 1),
                    TotalRows = s.TotalRows, RowsRead = s.RowsRead, RowsReady = s.RowsReady, RowsInserted = s.RowsInserted,
                    SkippedDuplicateInFile = s.SkippedDuplicateInFile, SkippedExisting = s.SkippedExisting,
                    RowsWithErrors = s.RowsWithErrors, Warnings = s.Warnings, BatchesDone = s.BatchesDone, BatchesFailed = s.BatchesFailed,
                    DuplicateCheckedAgainstDb = s.DuplicateCheckedAgainstDb, NetTotal = s.NetTotal, FromDate = s.FromDate, ToDate = s.ToDate,
                    Dealers = j.Dealers.Values.OrderByDescending(d => d.Rows).Select(d => new JcshDealerSummary { DealerCode = d.DealerCode, Rows = d.Rows, NetTotal = d.NetTotal }).ToList(),
                    Issues = s.Issues.ToList(), IssuesTruncated = s.IssuesTruncated,
                    IssueCounts = new Dictionary<string, int>(s.IssueCounts), Message = s.Message, Engine = s.Engine
                };
            }
        }

        private static void AddIssue(Job j, int rowNo, string level, string message, ParsedRow? r = null)
        {
            lock (j.Sync)
            {
                var s = j.Status;
                s.IssueCounts[level] = s.IssueCounts.GetValueOrDefault(level) + 1;
                var shown = j.LevelShown.GetValueOrDefault(level);
                if (shown >= MaxIssuesPerLevel) { s.IssuesTruncated = true; return; }
                j.LevelShown[level] = shown + 1;
                s.Issues.Add(new JcshIssue { RowNo = rowNo, Level = level, Message = message, UniqueKey = r?.UniqueKey, JobNo = r?.JobNo, ChassisNo = r?.ChassisNo });
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // RUN (background)
        // ═════════════════════════════════════════════════════════════════════
        private sealed class ParsedRow
        {
            public int RowNo;
            public object?[] Values = null!;
            public string UniqueKey = "";
            public string? JobNo;
            public string? ChassisNo;
            public string DealerCode = "";
            public decimal NetTotal;
            public DateTime? JobDate;
        }

        private async Task RunAsync(Job job, UploadInfo up, JcshRunOptions opt, int fromRow, int toRow, string userId)
        {
            var ct = job.Cts.Token;
            var st = job.Status;
            lock (job.Sync) { st.State = "Running"; st.Phase = "Opening workbook…"; }
            var importBatchId = Guid.NewGuid();
            SqlConnection? con = null;
            try
            {
                var cs = ConnectionString;
                var (configured, exists, destMsg) = await CheckDestinationAsync(ct);
                var useDb = configured && exists;
                if (!opt.DryRun && !useDb)
                    throw new InvalidOperationException(destMsg ?? "The service history table is not available.");
                lock (job.Sync) st.DuplicateCheckedAgainstDb = useDb;
                if (opt.DryRun && !useDb && destMsg != null) AddIssue(job, 0, "Warning", destMsg);

                if (useDb)
                {
                    con = await OpenAsync(cs!, ct);
                    if (!opt.DryRun) await AcquireLockAsync(con, ct);
                    await CreateStageAsync(con, ct);
                }

                // column index of every mapped field in the sheet
                var colIndex = new Dictionary<string, int>();
                foreach (var kv in opt.Mapping) colIndex[kv.Key] = up.Headers.IndexOf(kv.Value) + 1;
                var cols = JcshColumns.All;
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var nullText = new int[cols.Count];
                var batch = new List<ParsedRow>(opt.BatchSize);
                var batchNo = 0;
                var consecutiveFailures = 0;

                using var fs = File.OpenRead(up.Path);
                using var wb = new LargeExcelWorkbook(fs);
                var sheet = wb.Worksheets.First(s => s.Name == up.SheetName);
                lock (job.Sync) st.Phase = "Reading shared strings…";
                _ = wb.Worksheets.Count; // workbook open; shared strings load on first row read

                foreach (var row in sheet.StreamRows(fromRow))
                {
                    ct.ThrowIfCancellationRequested();
                    var rowNo = row.RowNumber();
                    if (rowNo > toRow) break;
                    lock (job.Sync) { st.RowsRead = rowNo - fromRow + 1; if (st.Phase.StartsWith("Reading")) st.Phase = opt.DryRun ? "Validating rows…" : "Importing rows…"; }

                    var pr = ParseRow(job, row, cols, colIndex, nullText);
                    if (pr == null) continue;   // error already recorded

                    if (!seen.Add(pr.UniqueKey.TrimEnd()))
                    {
                        lock (job.Sync) st.SkippedDuplicateInFile++;
                        AddIssue(job, rowNo, "Skipped", "Duplicate UniqueKey inside the file - the first row with this key is used.", pr);
                        continue;
                    }

                    batch.Add(pr);
                    if (batch.Count >= opt.BatchSize)
                    {
                        consecutiveFailures = await FlushAsync(job, batch, ++batchNo, opt.DryRun, useDb, cs, importBatchId, userId, up.FileName, () => con, c => con = c, consecutiveFailures, ct);
                        batch.Clear();
                    }
                }
                if (batch.Count > 0)
                {
                    consecutiveFailures = await FlushAsync(job, batch, ++batchNo, opt.DryRun, useDb, cs, importBatchId, userId, up.FileName, () => con, c => con = c, consecutiveFailures, ct);
                    batch.Clear();
                }

                // one summary line per column for the literal "NULL" texts stored as NULL
                for (var i = 0; i < cols.Count; i++)
                    if (nullText[i] > 0)
                        AddIssue(job, 0, "Info", $"{cols[i].Key}: {nullText[i]:N0} cell(s) contain the text \"NULL\" - stored as SQL NULL.");

                lock (job.Sync)
                {
                    st.RowsRead = st.TotalRows;
                    st.State = "Completed";
                    st.Phase = opt.DryRun ? "Validation complete" : "Import complete";
                    st.Message = opt.DryRun
                        ? $"{st.RowsReady:N0} row(s) ready to import. Nothing was written."
                        : $"{st.RowsInserted:N0} row(s) inserted into {TableName} (ImportBatchId {importBatchId}).";
                    if (st.BatchesFailed > 0) st.Message += $" {st.BatchesFailed} batch(es) failed - run Import again to retry; inserted rows are skipped.";
                }
            }
            catch (OperationCanceledException)
            {
                lock (job.Sync)
                {
                    st.State = "Cancelled";
                    st.Phase = "Stopped";
                    st.Message = opt.DryRun ? "Validation stopped." : $"Import stopped by user - {st.RowsInserted:N0} row(s) already committed stay in the table (ImportBatchId {importBatchId}); run Import again to continue (they are skipped).";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Jobcard servicehistory {Mode} failed for {File}", opt.DryRun ? "validation" : "import", up.FileName);
                lock (job.Sync)
                {
                    st.State = "Failed";
                    st.Phase = "Failed";
                    st.Message = (opt.DryRun ? "Validation failed: " : $"Import failed after {st.RowsInserted:N0} inserted row(s): ") + ex.GetBaseException().Message;
                }
            }
            finally
            {
                if (con != null) { try { await con.DisposeAsync(); } catch { /* ignore */ } }
                lock (job.Sync) { st.FinishedAt = DateTime.UtcNow; st.ElapsedSeconds = job.Clock.Elapsed.TotalSeconds; }
                job.Clock.Stop();
            }
        }

        // ─────────────────────────────────────────────────────────────── parse + validate one row
        private ParsedRow? ParseRow(Job job, LargeExcelRow row, IReadOnlyList<JcshColumn> cols, Dictionary<string, int> colIndex, int[] nullText)
        {
            var rowNo = row.RowNumber();
            var values = new object?[cols.Count];
            var errors = new List<string>();
            var warnings = new List<string>();

            for (var i = 0; i < cols.Count; i++)
            {
                var c = cols[i];
                if (!colIndex.TryGetValue(c.Key, out var ci) || ci <= 0) { values[i] = null; continue; }
                var v = row.Cell(ci).Value;
                if (v.IsBlank) { values[i] = null; continue; }
                if (v.IsError) { errors.Add($"{c.Key}: Excel error value (#N/A, #VALUE! …)"); continue; }
                if (v.IsText && v.ToString(CultureInfo.InvariantCulture).Trim().Equals("NULL", StringComparison.OrdinalIgnoreCase))
                {
                    nullText[i]++;
                    values[i] = null;
                    continue;
                }
                if (v.IsText && string.IsNullOrWhiteSpace(v.ToString(CultureInfo.InvariantCulture))) { values[i] = null; continue; }

                try { values[i] = ConvertCell(c, v, warnings); }
                catch (FormatException fe) { errors.Add($"{c.Key}: {fe.Message}"); }
            }

            var pr = new ParsedRow { RowNo = rowNo, Values = values };
            string? S(string key) { var idx = IndexOf(key); return idx >= 0 ? values[idx]?.ToString() : null; }
            pr.UniqueKey = S("UniqueKey") ?? "";
            pr.JobNo = S("JobNo");
            pr.ChassisNo = S("ChassisNo");
            pr.DealerCode = S("DealerCode") ?? "";
            pr.NetTotal = values[IndexOf("NetTotal")] is decimal nt ? nt : 0m;
            pr.JobDate = values[IndexOf("JobDate")] as DateTime?;

            foreach (var c in cols.Where(c => c.Required))
                if (values[IndexOf(c.Key)] == null && !errors.Any(e => e.StartsWith(c.Key + ":")))
                    errors.Add($"{c.Key} is blank (mandatory)");

            if (errors.Count > 0)
            {
                lock (job.Sync) job.Status.RowsWithErrors++;
                AddIssue(job, rowNo, "Error", string.Join("; ", errors), pr);
                return null;
            }

            if (pr.ChassisNo == null) warnings.Add("ChassisNo is blank");
            if (values[IndexOf("DealerCode")] is string dc && values[IndexOf("JobDate")] is DateTime jd && values[IndexOf("JobNo")] is int jn)
            {
                var composite = $"{dc}{jn}{jd:yyyy-MM-dd}{pr.ChassisNo}";
                if (!string.Equals(composite, pr.UniqueKey, StringComparison.Ordinal))
                    AddIssue(job, rowNo, "Info", $"UniqueKey differs from DealerCode+JobNo+JobDate+ChassisNo (\"{composite}\") - the file's UniqueKey is kept as-is.", pr);
            }
            if (warnings.Count > 0)
            {
                lock (job.Sync) job.Status.Warnings++;
                AddIssue(job, rowNo, "Warning", string.Join("; ", warnings), pr);
            }
            return pr;
        }

        private static readonly Dictionary<string, int> _index = JcshColumns.All.Select((c, i) => (c.Key, i)).ToDictionary(x => x.Key, x => x.i, StringComparer.OrdinalIgnoreCase);
        private static int IndexOf(string key) => _index[key];

        private static readonly string[] DateFormats =
        {
            "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "d-M-yyyy", "d/M/yyyy", "d.M.yyyy",
            "dd-MMM-yyyy", "d-MMM-yyyy", "dd MMM yyyy", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-ddTHH:mm:ss",
            "dd-MM-yyyy HH:mm", "dd/MM/yyyy HH:mm", "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss"
        };

        private static object ConvertCell(JcshColumn c, LargeExcelValue v, List<string> warnings)
        {
            var text = v.ToString(CultureInfo.InvariantCulture).Trim();
            switch (c.Type)
            {
                case JcshType.Text:
                {
                    string s;
                    if (v.IsNumber) s = NumberText(v.GetNumber());
                    else if (v.IsDateTime) s = v.GetDateTime().TimeOfDay == TimeSpan.Zero ? v.GetDateTime().ToString("yyyy-MM-dd") : v.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                    else s = v.ToString(CultureInfo.InvariantCulture);
                    if (s.Length > c.Length) throw new FormatException($"{s.Length} characters, column allows {c.Length} (value not truncated)");
                    return s;
                }
                case JcshType.Int:
                {
                    double d;
                    if (v.IsNumber) d = v.GetNumber();
                    else if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) throw new FormatException($"\"{Clip(text)}\" is not a whole number");
                    if (d != Math.Floor(d) || d < int.MinValue || d > int.MaxValue) throw new FormatException($"{d.ToString(CultureInfo.InvariantCulture)} is not a whole number in range");
                    return (int)d;
                }
                case JcshType.Decimal:
                {
                    decimal m;
                    if (v.IsNumber)
                    {
                        try { m = (decimal)v.GetNumber(); } catch (OverflowException) { throw new FormatException("number too large"); }
                    }
                    else if (!decimal.TryParse(text.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out m)) throw new FormatException($"\"{Clip(text)}\" is not a number");
                    var r = Math.Round(m, c.Scale, MidpointRounding.AwayFromZero);
                    if (r != m) warnings.Add($"{c.Key} {m.ToString(CultureInfo.InvariantCulture)} rounded to {c.Scale} decimals");
                    var maxInt = (decimal)Math.Pow(10, c.Precision - c.Scale);
                    if (Math.Abs(r) >= maxInt) throw new FormatException($"{r.ToString(CultureInfo.InvariantCulture)} exceeds decimal({c.Precision},{c.Scale})");
                    return r;
                }
                case JcshType.Date:
                case JcshType.DateTime:
                {
                    DateTime dt;
                    if (v.IsDateTime) dt = v.GetDateTime();
                    else if (v.IsNumber)
                    {
                        var n = v.GetNumber();
                        if (n <= 0 || n >= 2958466) throw new FormatException($"{n.ToString(CultureInfo.InvariantCulture)} is not a date");
                        dt = DateTime.FromOADate(n);
                    }
                    else if (!DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        throw new FormatException($"\"{Clip(text)}\" is not a date (use dd-MM-yyyy or a real Excel date)");
                    if (c.Type == JcshType.Date)
                    {
                        if (dt.TimeOfDay != TimeSpan.Zero) warnings.Add($"{c.Key} time part {dt:HH:mm:ss} dropped (date column)");
                        return dt.Date;
                    }
                    return dt;
                }
                case JcshType.Bit:
                {
                    if (v.IsBoolean) return v.GetBoolean();
                    if (v.IsNumber && (v.GetNumber() == 0 || v.GetNumber() == 1)) return v.GetNumber() == 1;
                    return text.ToLowerInvariant() switch
                    {
                        "1" or "true" or "yes" or "y" => true,
                        "0" or "false" or "no" or "n" => false,
                        _ => throw new FormatException($"\"{Clip(text)}\" is not 0/1/true/false")
                    };
                }
            }
            throw new FormatException("unsupported column type");
        }

        /// <summary>Excel number stored in a text column (mobile, reg no, chassis …) without "E+" or ".0".</summary>
        private static string NumberText(double d)
        {
            if (d == Math.Floor(d) && Math.Abs(d) < 1e18) return ((long)d).ToString(CultureInfo.InvariantCulture);
            try { return ((decimal)d).ToString(CultureInfo.InvariantCulture); }
            catch (OverflowException) { return d.ToString("R", CultureInfo.InvariantCulture); }
        }

        private static string Clip(string s) => s.Length > 40 ? s[..40] + "…" : s;

        // ─────────────────────────────────────────────────────────────── database
        private static async Task<SqlConnection> OpenAsync(string cs, CancellationToken ct)
        {
            var b = new SqlConnectionStringBuilder(cs) { ApplicationName = "DMS_BAPL JobCardServiceHistory import" };
            if (b.ConnectTimeout < 30) b.ConnectTimeout = 30;
            var con = new SqlConnection(b.ConnectionString);
            await con.OpenAsync(ct);
            return con;
        }

        private static async Task AcquireLockAsync(SqlConnection con, CancellationToken ct)
        {
            await using var cmd = new SqlCommand("sp_getapplock", con) { CommandType = CommandType.StoredProcedure, CommandTimeout = 30 };
            cmd.Parameters.AddWithValue("@Resource", AppLock);
            cmd.Parameters.AddWithValue("@LockMode", "Exclusive");
            cmd.Parameters.AddWithValue("@LockOwner", "Session");
            cmd.Parameters.AddWithValue("@LockTimeout", 0);
            var ret = cmd.Parameters.Add("@ret", SqlDbType.Int);
            ret.Direction = ParameterDirection.ReturnValue;
            await cmd.ExecuteNonQueryAsync(ct);
            if ((int)ret.Value < 0)
                throw new InvalidOperationException("Another Jobcard servicehistory import is running (database lock held). Try again later.");
        }

        private static string StageDdl()
        {
            var sb = new StringBuilder("CREATE TABLE #jcsh_stage (RowNo int NOT NULL");
            foreach (var c in JcshColumns.All)
                sb.Append($", [{c.Db}] {c.SqlType}{(c.Type == JcshType.Text ? " COLLATE DATABASE_DEFAULT" : "")} NULL");
            sb.Append(");");
            return sb.ToString();
        }

        private static async Task CreateStageAsync(SqlConnection con, CancellationToken ct)
        {
            await using var cmd = new SqlCommand("IF OBJECT_ID('tempdb..#jcsh_stage') IS NOT NULL DROP TABLE #jcsh_stage; " + StageDdl(), con) { CommandTimeout = 60 };
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static DataTable NewTable()
        {
            var t = new DataTable();
            t.Columns.Add("RowNo", typeof(int));
            foreach (var c in JcshColumns.All)
                t.Columns.Add(c.Db, c.Type switch
                {
                    JcshType.Int => typeof(int),
                    JcshType.Decimal => typeof(decimal),
                    JcshType.Date or JcshType.DateTime => typeof(DateTime),
                    JcshType.Bit => typeof(bool),
                    _ => typeof(string)
                });
            return t;
        }

        private static readonly string ColumnList = string.Join(", ", JcshColumns.All.Select(c => $"[{c.Db}]"));
        private static readonly string StageColumnList = string.Join(", ", JcshColumns.All.Select(c => $"s.[{c.Db}]"));

        private const string ExistingSql =
            "SELECT s.RowNo FROM #jcsh_stage s WHERE EXISTS (SELECT 1 FROM " + TableName + " t {0} WHERE t.UniqueKey = s.UniqueKey);";

        /// <summary>Checks one batch against the table and (real import) inserts the new rows in one transaction.
        /// Returns the new consecutive-failure count.</summary>
        private async Task<int> FlushAsync(Job job, List<ParsedRow> batch, int batchNo, bool dryRun, bool useDb, string? cs,
            Guid importBatchId, string userId, string fileName, Func<SqlConnection?> getCon, Action<SqlConnection> setCon, int consecutiveFailures, CancellationToken ct)
        {
            var st = job.Status;
            HashSet<int> existing = new();

            if (useDb)
            {
                var table = NewTable();
                foreach (var r in batch)
                {
                    var dr = table.NewRow();
                    dr[0] = r.RowNo;
                    for (var i = 0; i < r.Values.Length; i++) dr[i + 1] = r.Values[i] ?? DBNull.Value;
                    table.Rows.Add(dr);
                }

                for (var attempt = 1; ; attempt++)
                {
                    var con = getCon();
                    SqlTransaction? tx = null;
                    try
                    {
                        if (con == null || con.State != ConnectionState.Open)
                        {
                            con = await OpenAsync(cs!, ct);
                            setCon(con);
                            if (!dryRun) await AcquireLockAsync(con, ct);
                            await CreateStageAsync(con, ct);
                        }
                        lock (job.Sync) st.Phase = $"{(dryRun ? "Checking" : "Importing")} batch {batchNo} ({batch.Count:N0} rows)…";

                        await using (var trunc = new SqlCommand("TRUNCATE TABLE #jcsh_stage;", con)) await trunc.ExecuteNonQueryAsync(ct);
                        using (var bulk = new SqlBulkCopy(con) { DestinationTableName = "#jcsh_stage", BulkCopyTimeout = 600, BatchSize = batch.Count })
                        {
                            foreach (DataColumn dc in table.Columns) bulk.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                            await bulk.WriteToServerAsync(table, ct);
                        }

                        if (!dryRun) tx = (SqlTransaction)await con.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
                        existing.Clear();
                        await using (var cmd = new SqlCommand(string.Format(ExistingSql, dryRun ? "" : "WITH (UPDLOCK, HOLDLOCK)"), con, tx) { CommandTimeout = 300 })
                        await using (var rd = await cmd.ExecuteReaderAsync(ct))
                            while (await rd.ReadAsync(ct)) existing.Add(rd.GetInt32(0));

                        var inserted = 0;
                        if (!dryRun)
                        {
                            var sql = $"INSERT INTO {TableName} ({ColumnList}, ImportBatchId, ImportedBy, SourceFileName, SourceRowNo) " +
                                      $"SELECT {StageColumnList}, @batch, @by, @file, s.RowNo FROM #jcsh_stage s " +
                                      $"WHERE NOT EXISTS (SELECT 1 FROM {TableName} t WITH (UPDLOCK, HOLDLOCK) WHERE t.UniqueKey = s.UniqueKey);";
                            await using var ins = new SqlCommand(sql, con, tx) { CommandTimeout = 600 };
                            ins.Parameters.Add("@batch", SqlDbType.UniqueIdentifier).Value = importBatchId;
                            ins.Parameters.Add("@by", SqlDbType.NVarChar, 100).Value = Truncate(userId, 100);
                            ins.Parameters.Add("@file", SqlDbType.NVarChar, 260).Value = Truncate(fileName, 260);
                            inserted = await ins.ExecuteNonQueryAsync(ct);
                            await tx!.CommitAsync(ct);
                            if (inserted != batch.Count - existing.Count)
                                AddIssue(job, 0, "Warning", $"Batch {batchNo}: expected {batch.Count - existing.Count:N0} inserts, database reported {inserted:N0}.");
                        }
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        if (tx != null) { try { await tx.RollbackAsync(); } catch { /* connection gone */ } }
                        throw;
                    }
                    catch (Exception ex) when (attempt < 2)
                    {
                        if (tx != null) { try { await tx.RollbackAsync(); } catch { /* ignore */ } }
                        _logger.LogWarning(ex, "Jobcard servicehistory batch {Batch} attempt {Attempt} failed - retrying", batchNo, attempt);
                        try { con?.Dispose(); } catch { /* ignore */ }
                        setCon(null!);
                        await Task.Delay(TimeSpan.FromSeconds(5), ct);
                    }
                    catch (Exception ex)
                    {
                        if (tx != null) { try { await tx.RollbackAsync(); } catch { /* ignore */ } }
                        _logger.LogError(ex, "Jobcard servicehistory batch {Batch} failed", batchNo);
                        try { con?.Dispose(); } catch { /* ignore */ }
                        setCon(null!);
                        var msg = $"Batch {batchNo} (rows {batch[0].RowNo}-{batch[^1].RowNo}) failed and was rolled back: {ex.GetBaseException().Message}";
                        lock (job.Sync) { st.BatchesFailed++; st.RowsWithErrors += batch.Count; }
                        AddIssue(job, batch[0].RowNo, "Error", msg);
                        if (consecutiveFailures + 1 >= 3) throw new InvalidOperationException("3 batches in a row failed - import stopped. " + msg);
                        return consecutiveFailures + 1;
                    }
                }
            }

            // counts, dealer summary, skipped issues
            foreach (var r in batch)
            {
                if (existing.Contains(r.RowNo))
                {
                    lock (job.Sync) st.SkippedExisting++;
                    AddIssue(job, r.RowNo, "Skipped", "UniqueKey already in dbo.JobCardServiceHistory - existing row kept, not overwritten.", r);
                    continue;
                }
                lock (job.Sync)
                {
                    if (dryRun) st.RowsReady++; else st.RowsInserted++;
                    st.NetTotal += r.NetTotal;
                    if (r.JobDate is DateTime d)
                    {
                        if (st.FromDate == null || d < st.FromDate) st.FromDate = d;
                        if (st.ToDate == null || d > st.ToDate) st.ToDate = d;
                    }
                    if (!job.Dealers.TryGetValue(r.DealerCode, out var ds))
                        job.Dealers[r.DealerCode] = ds = new JcshDealerSummary { DealerCode = r.DealerCode };
                    ds.Rows++;
                    ds.NetTotal += r.NetTotal;
                }
            }
            lock (job.Sync) st.BatchesDone++;
            return 0;
        }

        private static string Truncate(string? s, int n) => string.IsNullOrEmpty(s) ? "" : s.Length <= n ? s : s[..n];

        // ─────────────────────────────────────────────────────────────── temp files
        private void CleanupOldUploads()
        {
            foreach (var u in _uploads.Values.Where(u => DateTime.UtcNow - u.UploadedAt > UploadTtl).ToList())
                if (!(_running?.Status.UploadId == u.Id && _running.Status.State is "Queued" or "Running") && _uploads.TryRemove(u.Id, out _))
                    TryDelete(u.Path);
            try
            {
                if (!Directory.Exists(TempDir)) return;
                var known = _uploads.Values.Select(u => u.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var f in Directory.GetFiles(TempDir, "*.xlsx"))
                    if (!known.Contains(f) && File.GetLastWriteTimeUtc(f) < DateTime.UtcNow - UploadTtl) TryDelete(f);
            }
            catch { /* best effort */ }
        }

        private void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not delete temp import file {Path}", path); }
        }
    }
}
