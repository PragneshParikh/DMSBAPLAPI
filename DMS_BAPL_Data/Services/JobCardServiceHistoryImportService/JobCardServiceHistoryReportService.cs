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

        private static (string Where, List<SqlParameter> Ps) ServiceHistoryFilter(JcshServiceHistoryQuery q)
        {
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
