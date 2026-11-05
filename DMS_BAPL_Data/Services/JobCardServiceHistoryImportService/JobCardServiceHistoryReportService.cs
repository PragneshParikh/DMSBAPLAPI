using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.JobCardServiceHistoryImportService
{
    public interface IJobCardServiceHistoryReportService
    {
        Task<JcshReportPage> ServiceHistoryAsync(JcshServiceHistoryQuery q, CancellationToken ct);
        Task<JcshReportPage> JobDataAsync(JcshJobDataQuery q, CancellationToken ct);
        Task<List<JcshDealerOption>> DealersAsync(CancellationToken ct);
        /// <summary>Type-ahead for the Service History box: chassis / reg numbers starting with what was typed.</summary>
        Task<List<JcshSuggestion>> SuggestAsync(string? term, CancellationToken ct);
        /// <summary>Vehicle History Card (JC-Print format) for the Service History search.</summary>
        Task<JcshHistoryCard> HistoryCardAsync(JcshServiceHistoryQuery q, CancellationToken ct);
        /// <summary>Writes all matching rows (all 63 source columns) to an .xlsx in <paramref name="output"/>; returns the row count.</summary>
        Task<int> ExportServiceHistoryAsync(JcshServiceHistoryQuery q, Stream output, CancellationToken ct);
        Task<int> ExportJobDataAsync(JcshJobDataQuery q, Stream output, CancellationToken ct);
    }

    /// <summary>
    /// Service History (search by Chassis No / Registration No) and Job Data (dealer + date range) reports on
    /// BAPLDMSvad.dbo.JobCardServiceHistory. Read-only; all filtering and paging is done in SQL (≈4.8 lakh rows)
    /// using the table's indexes (ChassisNo, RegNo, DealerCode+JobDate). Uses the API's DefaultConnection.
    /// </summary>
    public sealed class JobCardServiceHistoryReportService : IJobCardServiceHistoryReportService
    {
        public const int MaxPageSize = 200;
        /// <summary>Excel export limit - above this the user is asked to narrow the filter</summary>
        public const int MaxExportRows = 200_000;
        private const string Table = "dbo.JobCardServiceHistory";

        /// <summary>columns shown in the on-screen tables (the export has all 63 source columns)</summary>
        private static readonly string[] GridColumns =
        {
            "Id", "JobDate", "DealerCode", "CompName", "Location", "JobNo", "JobType", "ServiceHead", "JobCategory", "Model",
            "ChassisNo", "RegNo", "KMS", "PartyName", "Supervisor", "Technician", "DocType", "DocNo", "InvoiceDate",
            "Parts", "Labour", "TotalWOTax", "GSTAmount", "NetTotal", "JobStatus"
        };

        private static readonly object DealerCacheLock = new();
        private static (DateTime At, List<JcshDealerOption> List)? _dealerCache;

        private readonly IConfiguration _config;
        public JobCardServiceHistoryReportService(IConfiguration config) { _config = config; }

        private SqlConnection NewConnection()
        {
            var cs = _config.GetConnectionString(JobCardServiceHistoryImportService.ConnectionName);
            if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("DefaultConnection is not configured on the API.");
            return new SqlConnection(cs);
        }

        // ─────────────────────────────────────────────────────────────── filters
        /// <summary>Chassis / Reg No as typed → letters and digits only, upper case ("MH 12-ZH 4503" → "MH12ZH4503").</summary>
        public static string NormalizeSearch(string? s)
        {
            var t = new string((s ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            if (t.Length < 3) throw new ArgumentException("Enter at least 3 letters / digits of the Chassis No or Registration No.");
            if (t.Length > 30) throw new ArgumentException("Chassis No / Registration No is too long (max 30 characters).");
            return t;
        }

        // ─────────────────────────────────────────────────────────────── Reg No clean-up (display only)
        // The source data has Reg No values that are not registration numbers: the chassis number typed into
        // Reg No ("P6DEC12LPBF003845"), part of it ("P6DEC12CPB"), or junk ("na", "MH", "0", "A2").
        // Indian format: state (2 letters) + RTO (1-2 digits) + series (0-3 letters) + number (1-4 digits),
        // or Bharat series 22BH1234AA. Stored data is not changed.
        private static readonly System.Text.RegularExpressions.Regex RegNoPattern =
            new(@"^(?:[A-Z]{2}\d{1,2}[A-Z]{0,3}\d{1,4}|\d{2}BH\d{4}[A-Z]{1,2})$", System.Text.RegularExpressions.RegexOptions.Compiled);

        private static string Norm(string? s) => new string((s ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        /// <summary>true when <paramref name="reg"/> looks like a real registration number and is not (part of) the chassis no</summary>
        public static bool IsValidRegNo(string? reg, string? chassis)
        {
            var r = Norm(reg);
            if (r.Length < 5 || !RegNoPattern.IsMatch(r)) return false;
            var c = Norm(chassis);
            return c.Length == 0 || !(c.StartsWith(r) || r.StartsWith(c));
        }

        /// <summary>the vehicle's registration no: the valid value used on most jobs (ties → most recent job)</summary>
        private static string? BestRegNo(IEnumerable<(string? Reg, string? Chassis, DateTime? JobDate, int Jobs)> rows) =>
            rows.Where(x => IsValidRegNo(x.Reg, x.Chassis))
                .GroupBy(x => x.Reg!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => (Reg: g.First().Reg!.Trim(), Jobs: g.Sum(x => x.Jobs), Last: g.Max(x => x.JobDate ?? DateTime.MinValue)))
                .OrderByDescending(x => x.Jobs).ThenByDescending(x => x.Last)
                .Select(x => x.Reg).FirstOrDefault();

        private static (string Where, List<SqlParameter> Ps) ServiceHistoryFilter(JcshServiceHistoryQuery q)
        {
            if (q.Exact)
            {
                // a value picked from the type-ahead: match the stored value exactly (it may contain characters the
                // free-text search strips, e.g. "P6DEC,12CPEC020049"); equality also uses the ChassisNo / RegNo indexes
                var v = (q.Search ?? "").Trim();
                if (v.Length == 0 || v.Length > 30) throw new ArgumentException("Choose a Chassis No or Registration No.");
                return ("WHERE (ChassisNo = @v OR RegNo = @v)",
                        new List<SqlParameter> { new("@v", SqlDbType.NVarChar, 30) { Value = v } });
            }
            var s = NormalizeSearch(q.Search);
            // prefix match: uses IX_..._ChassisNo / IX_..._RegNo (index seek), only letters/digits so no LIKE wildcards
            return ("WHERE (ChassisNo LIKE @s + '%' OR RegNo LIKE @s + '%')",
                    new List<SqlParameter> { new("@s", SqlDbType.NVarChar, 30) { Value = s } });
        }

        private static (string Where, List<SqlParameter> Ps) JobDataFilter(JcshJobDataQuery q)
        {
            if (q.FromDate == null || q.ToDate == null) throw new ArgumentException("Choose a Start Date and an End Date.");
            if (q.ToDate.Value.Date < q.FromDate.Value.Date) throw new ArgumentException("End Date is before Start Date.");
            var ps = new List<SqlParameter>
            {
                new("@from", SqlDbType.Date) { Value = q.FromDate.Value.Date },
                new("@to", SqlDbType.Date) { Value = q.ToDate.Value.Date }
            };
            var where = "WHERE JobDate >= @from AND JobDate <= @to";
            if (!string.IsNullOrWhiteSpace(q.DealerCode))
            {
                where += " AND DealerCode = @dealer";
                ps.Add(new SqlParameter("@dealer", SqlDbType.NVarChar, 20) { Value = q.DealerCode.Trim() });
            }
            return (where, ps);
        }

        private static SqlParameter[] Clone(IEnumerable<SqlParameter> ps) =>
            ps.Select(p => new SqlParameter(p.ParameterName, p.SqlDbType, p.Size) { Value = p.Value }).ToArray();

        // ─────────────────────────────────────────────────────────────── pages
        public async Task<JcshReportPage> ServiceHistoryAsync(JcshServiceHistoryQuery q, CancellationToken ct)
        {
            var (where, ps) = ServiceHistoryFilter(q);
            return await PageAsync(where, ps, "JobDate DESC, Id DESC", q.Page, q.PageSize, withDealers: false, ct);
        }

        /// <summary>jobs per History Card print - a picked vehicle rarely has more than a few dozen</summary>
        public const int MaxCardJobs = 300;

        /// <summary>columns the Vehicle History Card (JC-Print format) needs</summary>
        private static readonly string[] CardColumns =
        {
            "Id", "ChassisNo", "RegNo", "Model", "DealerCode", "CompName", "Location", "JobNo", "JobDate", "JobType", "ServiceHead",
            "JobCategory", "RepairType", "PartyName", "MobileNumber", "KMS", "DocType", "DocNo", "InvoiceDate", "Technician", "Supervisor",
            "JobStatus", "SaleDate",
            "BatterySerialNo1", "BatterySerialNo2", "BatterySerialNo3", "BatterySerialNo4", "BatterySerialNo5", "BatterySerialNo6",
            "IndividualAHBattery1", "IndividualAHBattery2", "IndividualAHBattery3", "IndividualAHBattery4", "IndividualAHBattery5", "IndividualAHBattery6",
            "Parts", "Accessory", "Oil", "Labour", "OutsideWork", "TotalWOTax", "GSTAmount", "IGSTAmount", "NetTotal"
        };

        public async Task<JcshHistoryCard> HistoryCardAsync(JcshServiceHistoryQuery q, CancellationToken ct)
        {
            var (where, ps) = ServiceHistoryFilter(q);
            var card = new JcshHistoryCard { Limit = MaxCardJobs };

            await using var con = NewConnection();
            await con.OpenAsync(ct);

            await using (var cnt = new SqlCommand($"SELECT COUNT(*) FROM {Table} {where};", con) { CommandTimeout = 120 })
            {
                cnt.Parameters.AddRange(Clone(ps));
                card.TotalJobs = Convert.ToInt32(await cnt.ExecuteScalarAsync(ct));
            }
            card.Truncated = card.TotalJobs > MaxCardJobs;

            var cols = string.Join(", ", CardColumns.Select(c => $"[{c}]"));
            var rows = new List<Dictionary<string, object?>>();
            await using (var cmd = new SqlCommand($"SELECT TOP (@n) {cols} FROM {Table} {where} ORDER BY ChassisNo, JobDate DESC, JobNo DESC, Id DESC;", con) { CommandTimeout = 120 })
            {
                cmd.Parameters.AddRange(Clone(ps));
                cmd.Parameters.Add("@n", SqlDbType.Int).Value = MaxCardJobs;
                await using var rd = await cmd.ExecuteReaderAsync(ct);
                while (await rd.ReadAsync(ct))
                {
                    var row = new Dictionary<string, object?>(rd.FieldCount);
                    for (var i = 0; i < rd.FieldCount; i++) row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                    rows.Add(row);
                }
            }

            string? S(Dictionary<string, object?> r, string k) => r.TryGetValue(k, out var v) && v != null ? Convert.ToString(v)?.Trim() : null;

            foreach (var g in rows.GroupBy(r => S(r, "ChassisNo") ?? ""))
            {
                var jobs = g.ToList();
                var bestReg = BestRegNo(jobs.Select(r => (S(r, "RegNo"), S(r, "ChassisNo"), r["JobDate"] as DateTime?, 1)));
                // per job: keep the job's own Reg No when it is a real one; otherwise (chassis no / junk typed into
                // Reg No) show the vehicle's registration no from its other jobs; the raw value stays in RegNoAsEntered
                foreach (var r in jobs)
                {
                    var raw = S(r, "RegNo");
                    r["RegNoAsEntered"] = raw;
                    r["RegNo"] = IsValidRegNo(raw, S(r, "ChassisNo")) ? raw : bestReg;
                }
                card.Vehicles.Add(new JcshCardVehicle
                {
                    ChassisNo = g.Key.Length == 0 ? null : g.Key,
                    RegNo = bestReg,
                    Model = jobs.Select(r => S(r, "Model")).FirstOrDefault(v => !string.IsNullOrEmpty(v)),
                    SaleDate = jobs.Select(r => r["SaleDate"] as DateTime?).FirstOrDefault(v => v != null),
                    SaleSource = jobs.Any(r => r["SaleDate"] != null) ? "Import" : null,
                    Jobs = jobs
                });
            }

            // Vehicle Info from the DMS vehicle sale bill (latest non-deleted bill per chassis), when the vehicle was billed in DMS
            var chassis = card.Vehicles.Where(v => v.ChassisNo != null).Select(v => v.ChassisNo!).Take(100).ToList();
            if (chassis.Count > 0)
            {
                var names = chassis.Select((_, i) => "@c" + i).ToList();
                var sql = "SELECT d.ChassisNo, h.InvoiceNo, h.SaleBillNo, h.SaleDate, h.BookingId, d.Colour, h.DealerCode, dm.Compname " +
                          "FROM dbo.VehicleSaleBillDetails d JOIN dbo.VehicleSaleBillHeader h ON h.Id = d.VehicleSaleBillId " +
                          "LEFT JOIN dbo.DealerMaster dm ON dm.Dealercode = h.DealerCode " +
                          $"WHERE h.IsDeleted = 0 AND d.ChassisNo IN ({string.Join(", ", names)}) ORDER BY h.SaleDate DESC, h.Id DESC;";
                await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 60 };
                for (var i = 0; i < chassis.Count; i++) cmd.Parameters.Add(new SqlParameter(names[i], SqlDbType.NVarChar, 50) { Value = chassis[i] });
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                await using var rd = await cmd.ExecuteReaderAsync(ct);
                while (await rd.ReadAsync(ct))
                {
                    var c = rd.GetString(0).Trim();
                    if (!seen.Add(c)) continue;   // latest bill only
                    var v = card.Vehicles.FirstOrDefault(x => string.Equals(x.ChassisNo, c, StringComparison.OrdinalIgnoreCase));
                    if (v == null) continue;
                    v.SaleSource = "DMS";
                    v.SaleInvoiceNo = rd.IsDBNull(1) || string.IsNullOrWhiteSpace(rd.GetString(1)) ? (rd.IsDBNull(2) ? null : rd.GetString(2)) : rd.GetString(1);
                    v.SaleDate = rd.GetDateTime(3);
                    v.BookingId = rd.IsDBNull(4) ? null : rd.GetString(4);
                    v.Colour = rd.IsDBNull(5) ? null : rd.GetString(5);
                    v.SoldThrough = rd.IsDBNull(7) ? (rd.IsDBNull(6) ? null : rd.GetString(6)) : rd.GetString(7);
                }
            }
            return card;
        }

        public async Task<JcshReportPage> JobDataAsync(JcshJobDataQuery q, CancellationToken ct)
        {
            var (where, ps) = JobDataFilter(q);
            return await PageAsync(where, ps, "DealerCode, JobDate DESC, Id DESC", q.Page, q.PageSize, withDealers: true, ct);
        }

        private async Task<JcshReportPage> PageAsync(string where, List<SqlParameter> ps, string orderBy, int page, int pageSize, bool withDealers, CancellationToken ct)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize <= 0 ? 50 : pageSize, 1, MaxPageSize);
            var result = new JcshReportPage { Page = page, PageSize = pageSize };

            await using var con = NewConnection();
            await con.OpenAsync(ct);

            await using (var cnt = new SqlCommand($"SELECT COUNT_BIG(*), ISNULL(SUM(NetTotal), 0) FROM {Table} {where};", con) { CommandTimeout = 120 })
            {
                cnt.Parameters.AddRange(Clone(ps));
                await using var rd = await cnt.ExecuteReaderAsync(ct);
                if (await rd.ReadAsync(ct)) { result.Total = rd.GetInt64(0); result.NetTotal = rd.GetDecimal(1); }
            }

            // page on the narrow index first (Ids only), then read the full columns of just those rows -
            // keeps deep pages (e.g. page 9,000 of "all dealers, all dates") cheap
            var cols = string.Join(", ", GridColumns.Select(c => $"t.[{c}]"));
            var orderByT = string.Join(", ", orderBy.Split(',').Select(o => "t." + o.Trim()));
            var sql = $"WITH k AS (SELECT Id FROM {Table} {where} ORDER BY {orderBy} OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY) " +
                      $"SELECT {cols} FROM {Table} t JOIN k ON k.Id = t.Id ORDER BY {orderByT};";
            await using (var cmd = new SqlCommand(sql, con) { CommandTimeout = 120 })
            {
                cmd.Parameters.AddRange(Clone(ps));
                cmd.Parameters.Add("@skip", SqlDbType.Int).Value = (page - 1) * pageSize;
                cmd.Parameters.Add("@take", SqlDbType.Int).Value = pageSize;
                await using var rd = await cmd.ExecuteReaderAsync(ct);
                while (await rd.ReadAsync(ct))
                {
                    var row = new Dictionary<string, object?>(rd.FieldCount);
                    for (var i = 0; i < rd.FieldCount; i++) row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                    result.Rows.Add(row);
                }
            }

            if (withDealers)
            {
                var dealerSql = $"SELECT DealerCode, MAX(CompName), COUNT(*), COUNT(DISTINCT ChassisNo), ISNULL(SUM(NetTotal), 0), MIN(JobDate), MAX(JobDate) " +
                          $"FROM {Table} {where} GROUP BY DealerCode ORDER BY COUNT(*) DESC;";
                await using var cmd = new SqlCommand(dealerSql, con) { CommandTimeout = 120 };
                cmd.Parameters.AddRange(Clone(ps));
                await using var rd = await cmd.ExecuteReaderAsync(ct);
                while (await rd.ReadAsync(ct))
                    result.Dealers.Add(new JcshJobDataDealerRow
                    {
                        DealerCode = rd.GetString(0),
                        DealerName = rd.IsDBNull(1) ? null : rd.GetString(1),
                        Jobs = rd.GetInt32(2),
                        Vehicles = rd.GetInt32(3),
                        NetTotal = rd.GetDecimal(4),
                        FirstJobDate = rd.IsDBNull(5) ? null : rd.GetDateTime(5),
                        LastJobDate = rd.IsDBNull(6) ? null : rd.GetDateTime(6)
                    });
            }
            return result;
        }

        /// <summary>Dealers that have imported service history (code + name from the file's CompName). Cached 30 min.</summary>
        public async Task<List<JcshDealerOption>> DealersAsync(CancellationToken ct)
        {
            lock (DealerCacheLock)
                if (_dealerCache is { } c && DateTime.UtcNow - c.At < TimeSpan.FromMinutes(30)) return c.List;

            var list = new List<JcshDealerOption>();
            await using var con = NewConnection();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand($"SELECT DealerCode, MAX(CompName), COUNT(*) FROM {Table} GROUP BY DealerCode ORDER BY DealerCode;", con) { CommandTimeout = 120 };
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
                list.Add(new JcshDealerOption { DealerCode = rd.GetString(0), DealerName = rd.IsDBNull(1) ? null : rd.GetString(1), Jobs = rd.GetInt32(2) });

            lock (DealerCacheLock) _dealerCache = (DateTime.UtcNow, list);
            return list;
        }

        // ─────────────────────────────────────────────────────────────── type-ahead
        private const int SuggestPerKind = 15;

        /// <summary>
        /// Up to 10 chassis + 10 registration numbers that START with the typed text (letters / digits only, min 2).
        /// Two steps per kind so a short prefix like "P6" (matches most rows) stays fast:
        ///   1. TOP 10 distinct values from the index alone (stream aggregate, stops after 10)
        ///   2. details (reg / chassis, model, dealer, jobs, last job date) for just those 10 values.
        /// </summary>
        public async Task<List<JcshSuggestion>> SuggestAsync(string? term, CancellationToken ct)
        {
            var s = new string((term ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            if (s.Length < 2) return new List<JcshSuggestion>();
            if (s.Length > 30) s = s[..30];

            var result = new List<JcshSuggestion>();
            await using var con = NewConnection();
            await con.OpenAsync(ct);
            foreach (var (col, other, kind) in new[] { ("ChassisNo", "RegNo", "Chassis No"), ("RegNo", "ChassisNo", "Reg No") })
            {
                var isReg = col == "RegNo";
                var values = new List<string>();
                await using (var cmd = new SqlCommand(
                    $"SELECT TOP (@n) {col} FROM {Table} WHERE {col} LIKE @s + '%' GROUP BY {col} ORDER BY {col};", con) { CommandTimeout = 30 })
                {
                    // Reg No: read extra, then drop values that are not registration numbers (chassis no / junk typed in)
                    cmd.Parameters.Add("@n", SqlDbType.Int).Value = isReg ? SuggestPerKind * 4 : SuggestPerKind;
                    cmd.Parameters.Add("@s", SqlDbType.NVarChar, 30).Value = s;
                    await using var rd = await cmd.ExecuteReaderAsync(ct);
                    while (await rd.ReadAsync(ct))
                    {
                        var v = rd.GetString(0);
                        if (!isReg || IsValidRegNo(v, null)) values.Add(v);
                    }
                }
                values = values.Take(SuggestPerKind).ToList();
                if (values.Count == 0) continue;

                // details per (value, other number) so the other number can be cleaned up as well
                var rows = new List<(string Val, string? Other, string? Model, string? Dealer, int Jobs, DateTime? Last)>();
                var names = values.Select((_, i) => "@v" + i).ToList();
                await using (var cmd = new SqlCommand(
                    $"SELECT {col}, {other}, MAX(Model), MAX(DealerCode), COUNT(*), MAX(JobDate) FROM {Table} " +
                    $"WHERE {col} IN ({string.Join(", ", names)}) GROUP BY {col}, {other};", con) { CommandTimeout = 30 })
                {
                    for (var i = 0; i < values.Count; i++) cmd.Parameters.Add(names[i], SqlDbType.NVarChar, 30).Value = values[i];
                    await using var rd = await cmd.ExecuteReaderAsync(ct);
                    while (await rd.ReadAsync(ct))
                        rows.Add((rd.GetString(0), rd.IsDBNull(1) ? null : rd.GetString(1), rd.IsDBNull(2) ? null : rd.GetString(2),
                                  rd.IsDBNull(3) ? null : rd.GetString(3), rd.GetInt32(4), rd.IsDBNull(5) ? null : rd.GetDateTime(5)));
                }

                foreach (var g in rows.GroupBy(r => r.Val).OrderBy(g => g.Key, StringComparer.Ordinal))
                {
                    var latest = g.OrderByDescending(r => r.Last ?? DateTime.MinValue).First();
                    // chassis suggestion → the vehicle's real Reg No; reg suggestion → its (most used) chassis no
                    var o = isReg
                        ? g.Where(r => !string.IsNullOrWhiteSpace(r.Other)).OrderByDescending(r => r.Jobs).Select(r => r.Other).FirstOrDefault()
                        : BestRegNo(g.Select(r => (r.Other, (string?)r.Val, r.Last, r.Jobs)));
                    result.Add(new JcshSuggestion
                    {
                        Value = g.Key,
                        Kind = kind,
                        ChassisNo = isReg ? o : g.Key,
                        RegNo = isReg ? g.Key : o,
                        Model = latest.Model ?? g.Select(r => r.Model).FirstOrDefault(m => m != null),
                        DealerCode = latest.Dealer,
                        Jobs = g.Sum(r => r.Jobs),
                        LastJobDate = latest.Last
                    });
                }
            }
            return result;
        }

        // ─────────────────────────────────────────────────────────────── Excel export
        public async Task<int> ExportServiceHistoryAsync(JcshServiceHistoryQuery q, Stream output, CancellationToken ct)
        {
            var (where, ps) = ServiceHistoryFilter(q);
            return await ExportAsync(where, ps, "JobDate DESC, Id DESC", "Service History", output, ct);
        }

        public async Task<int> ExportJobDataAsync(JcshJobDataQuery q, Stream output, CancellationToken ct)
        {
            var (where, ps) = JobDataFilter(q);
            return await ExportAsync(where, ps, "DealerCode, JobDate DESC, Id DESC", "Job Data", output, ct);
        }

        private async Task<int> ExportAsync(string where, List<SqlParameter> ps, string orderBy, string sheetName, Stream output, CancellationToken ct)
        {
            await using var con = NewConnection();
            await con.OpenAsync(ct);

            long count;
            await using (var cnt = new SqlCommand($"SELECT COUNT_BIG(*) FROM {Table} {where};", con) { CommandTimeout = 120 })
            {
                cnt.Parameters.AddRange(Clone(ps));
                count = (long)(await cnt.ExecuteScalarAsync(ct) ?? 0L);
            }
            if (count > MaxExportRows)
                throw new ArgumentException($"{count:N0} records match - the Excel export allows up to {MaxExportRows:N0}. Choose one dealer or a shorter date range.");

            // all 63 source columns, Excel headers exactly as in "Jobcard servicehistory.xlsx" (SourceId → "Id")
            var cols = JcshColumns.All;
            var select = string.Join(", ", cols.Select(c => $"[{c.Db}]"));
            await using var cmd = new SqlCommand($"SELECT {select} FROM {Table} {where} ORDER BY {orderBy};", con) { CommandTimeout = 600 };
            cmd.Parameters.AddRange(Clone(ps));
            await using var rd = await cmd.ExecuteReaderAsync(ct);

            var written = 0;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            using (var doc = SpreadsheetDocument.Create(output, SpreadsheetDocumentType.Workbook))
            {
                var wbPart = doc.AddWorkbookPart();
                var stylesPart = wbPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = BuildStyles();
                stylesPart.Stylesheet.Save();
                var wsPart = wbPart.AddNewPart<WorksheetPart>();

                // The sheet XML is written directly (no OpenXML object per cell - that was ~40 s for 26k rows).
                // Inline strings, real numbers and real dates; every cell is written (empty <c/> for NULL) so columns stay aligned.
                await using (var part = wsPart.GetStream(FileMode.Create, FileAccess.Write))
                using (var x = System.Xml.XmlWriter.Create(part, new System.Xml.XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
                {
                    const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                    x.WriteStartDocument(true);
                    x.WriteStartElement("worksheet", ns);
                    // freeze the header row
                    x.WriteStartElement("sheetViews", ns); x.WriteStartElement("sheetView", ns); x.WriteAttributeString("workbookViewId", "0");
                    x.WriteStartElement("pane", ns);
                    x.WriteAttributeString("ySplit", "1"); x.WriteAttributeString("topLeftCell", "A2");
                    x.WriteAttributeString("activePane", "bottomLeft"); x.WriteAttributeString("state", "frozen");
                    x.WriteEndElement(); x.WriteEndElement(); x.WriteEndElement();
                    x.WriteStartElement("sheetData", ns);

                    void Text(string s, uint style)
                    {
                        x.WriteStartElement("c", ns);
                        x.WriteAttributeString("t", "inlineStr");
                        if (style != 0) x.WriteAttributeString("s", style.ToString(inv));
                        x.WriteStartElement("is", ns); x.WriteStartElement("t", ns);
                        if (s.Length > 0 && (char.IsWhiteSpace(s[0]) || char.IsWhiteSpace(s[^1]))) x.WriteAttributeString("xml", "space", null, "preserve");
                        x.WriteString(CleanXml(s));
                        x.WriteEndElement(); x.WriteEndElement(); x.WriteEndElement();
                    }
                    void Value(string v, uint style, string? type = null)
                    {
                        x.WriteStartElement("c", ns);
                        if (type != null) x.WriteAttributeString("t", type);
                        if (style != 0) x.WriteAttributeString("s", style.ToString(inv));
                        x.WriteElementString("v", ns, v);
                        x.WriteEndElement();
                    }

                    x.WriteStartElement("row", ns);
                    foreach (var c in cols) Text(c.Key, StyleHeader);
                    x.WriteEndElement();

                    var values = new object[cols.Count];
                    while (await rd.ReadAsync(ct))
                    {
                        rd.GetValues(values);
                        x.WriteStartElement("row", ns);
                        for (var i = 0; i < cols.Count; i++)
                        {
                            var v = values[i];
                            if (v is DBNull) { x.WriteStartElement("c", ns); x.WriteEndElement(); continue; }
                            switch (cols[i].Type)
                            {
                                case JcshType.Int: Value(((int)v).ToString(inv), 0); break;
                                case JcshType.Decimal: Value(((decimal)v).ToString(inv), 0); break;
                                case JcshType.Date: Value(((DateTime)v).ToOADate().ToString(inv), StyleDate); break;
                                case JcshType.DateTime: Value(((DateTime)v).ToOADate().ToString(inv), StyleDateTime); break;
                                case JcshType.Bit: Value((bool)v ? "1" : "0", 0, "b"); break;
                                default: Text((string)v, 0); break;
                            }
                        }
                        x.WriteEndElement();
                        written++;
                    }
                    x.WriteEndElement(); // sheetData
                    x.WriteEndElement(); // worksheet
                    x.WriteEndDocument();
                }

                wbPart.Workbook = new Workbook(new Sheets(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = 1U, Name = sheetName }));
                wbPart.Workbook.Save();
            }
            return written;
        }

        private const uint StyleHeader = 1, StyleDate = 2, StyleDateTime = 3;

        private static Stylesheet BuildStyles() => new(
            new NumberingFormats(
                new NumberingFormat { NumberFormatId = 164U, FormatCode = "dd.mm.yyyy" },
                new NumberingFormat { NumberFormatId = 165U, FormatCode = "dd.mm.yyyy hh:mm" }) { Count = 2U },
            new Fonts(new Font(), new Font(new Bold())) { Count = 2U },
            new Fills(new Fill(new PatternFill { PatternType = PatternValues.None }),
                      new Fill(new PatternFill { PatternType = PatternValues.Gray125 })) { Count = 2U },
            new Borders(new Border()) { Count = 1U },
            new CellStyleFormats(new CellFormat { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U }) { Count = 1U },
            new CellFormats(
                new CellFormat { FormatId = 0U },
                new CellFormat { FontId = 1U, FormatId = 0U, ApplyFont = true },
                new CellFormat { NumberFormatId = 164U, FormatId = 0U, ApplyNumberFormat = true },
                new CellFormat { NumberFormatId = 165U, FormatId = 0U, ApplyNumberFormat = true }) { Count = 4U },
            new CellStyles(new CellStyle { Name = "Normal", FormatId = 0U, BuiltinId = 0U }) { Count = 1U });

        /// <summary>XML 1.0 does not allow most control characters - drop them so the .xlsx always opens.</summary>
        private static string CleanXml(string s)
        {
            if (s.All(ch => ch >= 0x20 || ch == '\t' || ch == '\n' || ch == '\r')) return s;
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s) if (ch >= 0x20 || ch == '\t' || ch == '\n' || ch == '\r') sb.Append(ch);
            return sb.ToString();
        }
    }
}
