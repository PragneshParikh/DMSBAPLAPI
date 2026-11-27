using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Helpers;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DMS_BAPL_Data.Services.ErpMasterSyncService
{
    /// <summary>
    /// 26.11.2026 - see IErpMasterSyncService. DMS tables: dbo.DealerMaster, dbo.LocationMaster (BAPLDMSvad).
    /// ERP tables (connection string "BaplConnection", database baplfinal): dbo.C_CustomerMaster, dbo.C_DMS_DealerConsigneeDetail.
    /// New dealers go through IDealerMasterService.AddDealerAsync, so they also get their ledger and the login with
    /// the "Dealer" role, exactly like a dealer created on the page.
    /// </summary>
    public class ErpMasterSyncService : IErpMasterSyncService
    {
        public const string ErpConnectionName = "BaplConnection";

        // exact column headers of the UAT ERP reports (header text must not be changed)
        private static readonly string[] DealerHeaders = { "Company Code", "Company Name", "Dealer Code", "Email Id", "MobileNo", "State", "City", "Current Status" };
        private static readonly string[] LocationHeaders = { "Location Area", "Location Code", "Location Name", "Dealer Code", "Status" };

        private readonly BapldmsvadContext _db;
        private readonly IDealerMasterService _dealerService;
        private readonly IConfiguration _config;
        private readonly ILogger<ErpMasterSyncService> _logger;

        public ErpMasterSyncService(BapldmsvadContext db, IDealerMasterService dealerService, IConfiguration config,
            ILogger<ErpMasterSyncService> logger)
        {
            _db = db;
            _dealerService = dealerService;
            _config = config;
            _logger = logger;
        }

        // ===================================================================== helpers
        private static string Code(string? s) => (s ?? "").Trim().ToUpperInvariant();

        /// <summary>trims, treats the ERP's literal "NULL" as empty, drops a trailing ".0" of numbers stored as float</summary>
        private static string? Clean(string? s)
        {
            if (s == null) return null;
            s = s.Trim();
            if (s.Length == 0 || s.Equals("NULL", StringComparison.OrdinalIgnoreCase)) return null;
            if (Regex.IsMatch(s, @"^\d+\.0+$")) s = s[..s.IndexOf('.')];
            return s;
        }

        private static string? Cut(string? s, int max) => s == null ? null : (s.Length > max ? s[..max] : s);

        private static bool Same(string? a, string? b) =>
            string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

        private static bool IsActiveText(string? s)
        {
            var t = Regex.Replace(s ?? "", @"\s", "").ToLowerInvariant();
            return t == "active" || t == "y" || t == "yes" || t == "1" || t == "true";
        }

        /// <summary>row values of the first sheet keyed by the exact header text; throws with a clear message when a header is missing</summary>
        private static List<(int Row, Dictionary<string, string?> Values)> ReadSheet(IFormFile file, string[] requiredHeaders, string[] optionalHeaders)
        {
            if (file == null || file.Length == 0) throw new InvalidOperationException("Please choose the Excel file.");
            LargeExcelWorkbook wb;
            try { wb = new LargeExcelWorkbook(file.OpenReadStream()); }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The file could not be read as an Excel workbook (.xlsx). If it is an old .xls file, open it in Excel and save it as .xlsx. " + ex.Message);
            }
            using (wb)
            {
                var ws = wb.Worksheets[0];
                var header = ws.Row(1);
                var colOf = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var last = header.LastCellUsed()?.Address.ColumnNumber ?? 0;
                for (var c = 1; c <= last; c++)
                {
                    var h = header.Cell(c).Value.ToString(CultureInfo.InvariantCulture)?.Trim();
                    if (!string.IsNullOrEmpty(h) && !colOf.ContainsKey(h)) colOf[h] = c;
                }
                var missing = requiredHeaders.Where(h => !colOf.ContainsKey(h)).ToList();
                if (missing.Count > 0)
                    throw new InvalidOperationException($"Sheet '{ws.Name}': column(s) {string.Join(", ", missing.Select(m => $"'{m}'"))} not found. Use the ERP report as downloaded - do not change the column headers.");

                var wanted = requiredHeaders.Concat(optionalHeaders).Where(colOf.ContainsKey).ToList();
                var rows = new List<(int, Dictionary<string, string?>)>();
                foreach (var r in ws.StreamRows(2))
                {
                    if (r.IsEmpty()) continue;
                    var vals = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                    var any = false;
                    foreach (var h in wanted)
                    {
                        var v = r.Cell(colOf[h]).Value;
                        string? s = v.IsBlank ? null
                            : v.IsDateTime ? v.GetDateTime().ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
                            : v.IsNumber ? v.GetNumber().ToString("0.##########", CultureInfo.InvariantCulture)
                            : v.ToString(CultureInfo.InvariantCulture);
                        s = Clean(s);
                        vals[h] = s;
                        if (s != null) any = true;
                    }
                    if (any) rows.Add((r.RowNumber(), vals));
                }
                return rows;
            }
        }

        private static string? Val(Dictionary<string, string?> row, string header) => row.TryGetValue(header, out var v) ? v : null;

        private SqlConnection ErpConnection()
        {
            var cs = _config.GetConnectionString(ErpConnectionName);
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException($"Connection string '{ErpConnectionName}' is not configured on the API server.");
            return new SqlConnection(cs);
        }

        /// <summary>all rows of an ERP query as column -> cleaned text</summary>
        private async Task<List<Dictionary<string, string?>>> ErpRowsAsync(string sql, Action<SqlCommand>? addParams, CancellationToken ct)
        {
            var list = new List<Dictionary<string, string?>>();
            await using var conn = ErpConnection();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 180 };
            addParams?.Invoke(cmd);
            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            var names = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToArray();
            while (await rdr.ReadAsync(ct))
            {
                var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < names.Length; i++)
                {
                    if (rdr.IsDBNull(i)) { row[names[i]] = null; continue; }
                    var o = rdr.GetValue(i);
                    row[names[i]] = Clean(o switch
                    {
                        DateTime d => d.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                        _ => o.ToString()
                    });
                }
                list.Add(row);
            }
            return list;
        }

        private static void RequireColumns(List<Dictionary<string, string?>> rows, string table, params string[] cols)
        {
            if (rows.Count == 0) return;
            var missing = cols.Where(c => !rows[0].ContainsKey(c)).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException($"ERP table {table} has no column(s) {string.Join(", ", missing)}. Columns found: {string.Join(", ", rows[0].Keys)}");
        }

        private static DateTime? ParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            string[] f = { "dd-MM-yyyy", "dd/MM/yyyy", "d-M-yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy hh:mm:ss tt" };
            if (DateTime.TryParseExact(s.Trim(), f, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var oa) && oa > 20000 && oa < 80000) return DateTime.FromOADate(oa);
            return null;
        }

        private static void Count(ErpSyncResult r, string action)
        {
            switch (action)
            {
                case "Insert": r.Inserted++; break;
                case "Update": r.Updated++; break;
                case "Activate": r.Activated++; break;
                case "Inactivate": r.Inactivated++; break;
                case "Unchanged": r.Unchanged++; break;
                case "Skip": r.Skipped++; break;
                case "Failed": r.Failed++; break;
            }
        }

        private static ErpSyncItem Add(ErpSyncResult r, string action, string code, string? name, string? detail, int? row = null, string? dealer = null)
        {
            var it = new ErpSyncItem { Action = action, Code = code, Name = name, Detail = detail, RowNumber = row, DealerCode = dealer };
            r.Items.Add(it);
            Count(r, action);
            return it;
        }

        // ===================================================================== dealers: UAT report import
        private sealed class DealerRow
        {
            public int Row; public string Code = ""; public string? CompanyCode; public string? Name; public string? Email;
            public string? Mobile; public string? State; public string? City; public bool Active; public string? RegDate;
        }

        public async Task<ErpSyncResult> ImportUatDealersAsync(IFormFile file, bool apply, string userId, CancellationToken ct)
        {
            var result = new ErpSyncResult { Applied = apply, Source = "UAT ERP Manage Dealer Report: " + file?.FileName };
            var sheet = ReadSheet(file!, DealerHeaders, new[] { "Registration Date", "Source", "City Tier" });
            result.TotalRows = sheet.Count;

            // 1. sheet rows -> one row per dealer code
            var rows = new Dictionary<string, DealerRow>(StringComparer.OrdinalIgnoreCase);
            foreach (var (rowNo, v) in sheet)
            {
                var code = Code(Val(v, "Dealer Code"));
                if (code.Length == 0) { Add(result, "Skip", "", Val(v, "Company Name"), "Dealer Code is blank", rowNo); continue; }
                if (!Regex.IsMatch(code, @"^CUS\d+$"))
                {
                    Add(result, "Skip", code, Val(v, "Company Name"), "Not a dealer code (CUS....) - not imported", rowNo);
                    continue;
                }
                if (rows.ContainsKey(code)) result.Notes.Add($"Row {rowNo}: Dealer Code {code} appears again - the last row is used.");
                rows[code] = new DealerRow
                {
                    Row = rowNo, Code = code, CompanyCode = Val(v, "Company Code"), Name = Val(v, "Company Name"), Email = Val(v, "Email Id"),
                    Mobile = Val(v, "MobileNo"), State = Val(v, "State"), City = Val(v, "City"),
                    Active = IsActiveText(Val(v, "Current Status")), RegDate = Val(v, "Registration Date")
                };
            }

            // 2. compare with DealerMaster
            var dealers = await _db.DealerMasters.ToListAsync(ct);
            var byCode = dealers.GroupBy(d => Code(d.Dealercode)).ToDictionary(g => g.Key, g => g.First());
            var updates = new List<(DealerMaster D, DealerRow R)>();
            var inserts = new List<DealerRow>();

            foreach (var r in rows.Values.OrderBy(x => x.Code))
            {
                if (!byCode.TryGetValue(r.Code, out var d))
                {
                    inserts.Add(r);
                    Add(result, "Insert", r.Code, r.Name, $"New dealer ({(r.Active ? "Active" : "Inactive")}) - with ledger and Dealer login", r.Row, r.Code);
                    continue;
                }
                var changes = new List<string>();
                if (r.Name != null && !Same(d.Compname, r.Name)) changes.Add($"Name '{d.Compname}' -> '{r.Name}'");
                if (r.Mobile != null && !Same(d.Mobile, r.Mobile)) changes.Add($"Mobile {d.Mobile} -> {r.Mobile}");
                if (r.City != null && !Same(d.City, r.City)) changes.Add($"City {d.City} -> {r.City}");
                if (r.State != null && !Same(d.State, r.State)) changes.Add($"State {d.State} -> {r.State}");
                if (r.Email != null && string.IsNullOrWhiteSpace(d.Email)) changes.Add($"Email (blank) -> {r.Email}");
                else if (r.Email != null && !Same(d.Email, r.Email))
                    result.Notes.Add($"{r.Code}: Email differs (DMS {d.Email}, report {r.Email}) - kept, because the dealer login uses it.");

                string action;
                if (d.IsActive != r.Active) action = r.Active ? "Activate" : "Inactivate";
                else action = changes.Count > 0 ? "Update" : "Unchanged";
                if (action != "Unchanged") updates.Add((d, r));
                Add(result, action, r.Code, r.Name ?? d.Compname,
                    string.Join("; ", (d.IsActive != r.Active ? new[] { $"Status {(d.IsActive ? "Active" : "Inactive")} -> {(r.Active ? "Active" : "Inactive")}" } : Array.Empty<string>()).Concat(changes)),
                    r.Row, r.Code);
            }

            var toInactivate = dealers.Where(d => d.IsActive && !rows.ContainsKey(Code(d.Dealercode))).OrderBy(d => d.Dealercode).ToList();
            foreach (var d in toInactivate)
                Add(result, "Inactivate", Code(d.Dealercode), d.Compname, "Not in the UAT ERP dealer report");

            var activeAfter = dealers.Count(d => rows.TryGetValue(Code(d.Dealercode), out var r) ? r.Active : false) + inserts.Count(i => i.Active);
            result.ActiveAfter = activeAfter;
            result.InactiveAfter = dealers.Count + inserts.Count - activeAfter;
            if (!apply) return result;

            // 3a. new dealers - one at a time through the normal "create dealer" path (ledger + Dealer-role login)
            foreach (var r in inserts)
            {
                var item = result.Items.First(i => i.Action == "Insert" && i.Code == r.Code);
                if (string.IsNullOrWhiteSpace(r.Email) || string.IsNullOrWhiteSpace(r.State) || string.IsNullOrWhiteSpace(r.City) || string.IsNullOrWhiteSpace(r.Name))
                {
                    item.Action = "Failed"; item.Detail = "Company Name, Email Id, State and City are required for a new dealer.";
                    result.Inserted--; result.Failed++;
                    continue;
                }
                try
                {
                    var reg = ParseDate(r.RegDate) ?? DateTime.Now;
                    await _dealerService.AddDealerAsync(new DealerMasterViewModel
                    {
                        Compname = Cut(r.Name, 100)!,
                        Compcode = Cut(r.CompanyCode ?? r.Code, 100)!,
                        Dealercode = r.Code,
                        Adress1 = Cut(r.City, 200)!,
                        Adress2 = "",
                        City = Cut(r.City, 100)!,
                        State = Cut(r.State, 100)!,
                        Pin = "",
                        PhoneOff = "",
                        Mobile = Cut(r.Mobile, 15) ?? "",
                        Email = Cut(r.Email, 100)!,
                        Contactperson = Cut(r.Name, 100)!,
                        RegDate = reg.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                        TradCert = "",
                        CompgstinNo = "",
                        IsActive = r.Active,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }, userId);
                }
                catch (Exception ex)
                {
                    _db.ChangeTracker.Clear();   // nothing of a failed dealer may be saved later
                    item.Action = "Failed"; item.Detail = ex.GetBaseException().Message;
                    result.Inserted--; result.Failed++;
                    _logger.LogWarning(ex, "UAT dealer import: could not add {Code}", r.Code);
                }
            }

            // 3b. status / detail updates and inactivations - one transaction
            _db.ChangeTracker.Clear();
            var tracked = (await _db.DealerMasters.ToListAsync(ct)).GroupBy(d => Code(d.Dealercode)).ToDictionary(g => g.Key, g => g.First());
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var now = DateTime.Now;
            foreach (var (d0, r) in updates)
            {
                if (!tracked.TryGetValue(r.Code, out var d)) continue;
                d.IsActive = r.Active;
                if (r.Name != null) d.Compname = Cut(r.Name, 100)!;
                if (r.Mobile != null) d.Mobile = Cut(r.Mobile, 15)!;
                if (r.City != null) d.City = Cut(r.City, 100)!;
                if (r.State != null) d.State = Cut(r.State, 100)!;
                if (r.Email != null && string.IsNullOrWhiteSpace(d.Email)) d.Email = Cut(r.Email, 100)!;
                d.UpdatedBy = Cut(userId, 100);
                d.UpdatedDate = now;
            }
            foreach (var d0 in toInactivate)
            {
                if (!tracked.TryGetValue(Code(d0.Dealercode), out var d)) continue;
                d.IsActive = false;
                d.UpdatedBy = Cut(userId, 100);
                d.UpdatedDate = now;
            }
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            result.ActiveAfter = await _db.DealerMasters.CountAsync(d => d.IsActive, ct);
            result.InactiveAfter = await _db.DealerMasters.CountAsync(d => !d.IsActive, ct);
            return result;
        }

        // ===================================================================== dealers: ERP sync
        public async Task<ErpSyncResult> SyncDealersFromErpAsync(string? dealerCode, bool apply, string userId, CancellationToken ct)
        {
            var result = new ErpSyncResult { Applied = apply, Source = "ERP C_CustomerMaster (CustomerCode like '%CUS%' and Active = 'Y')" };
            var one = Code(dealerCode);
            var sql = "SELECT * FROM dbo.C_CustomerMaster WHERE CustomerCode LIKE '%CUS%' AND Active = 'Y'" + (one.Length > 0 ? " AND CustomerCode = @code" : "");
            var erp = await ErpRowsAsync(sql, c => { if (one.Length > 0) c.Parameters.Add("@code", SqlDbType.NVarChar, 50).Value = one; }, ct);
            RequireColumns(erp, "C_CustomerMaster", "CustomerCode", "CustomerName", "Address1", "ZipCode", "Mobile", "Email");
            result.TotalRows = erp.Count;
            var erpBy = erp.Where(e => Code(e.GetValueOrDefault("CustomerCode")).Length > 0)
                .GroupBy(e => Code(e["CustomerCode"])).ToDictionary(g => g.Key, g => g.First());

            var q = _db.DealerMasters.AsQueryable();
            if (one.Length > 0) q = q.Where(d => d.Dealercode == one);
            var dealers = await q.ToListAsync(ct);
            var now = DateTime.Now;

            foreach (var d in dealers.Where(x => x.IsActive).OrderBy(x => x.Dealercode))
            {
                var code = Code(d.Dealercode);
                if (!erpBy.TryGetValue(code, out var e))
                {
                    Add(result, "Skip", code, d.Compname, "Active in DMS but not active in ERP C_CustomerMaster");
                    continue;
                }
                var changes = new List<string>();
                void Set(string label, string? erpValue, int max, Func<string?> get, Action<string> set, bool onlyIfBlank = false)
                {
                    var v = Cut(Clean(erpValue), max);
                    if (v == null) return;
                    var cur = get();
                    if (onlyIfBlank && !string.IsNullOrWhiteSpace(cur)) return;
                    if (Same(cur, v)) return;
                    changes.Add($"{label} '{cur}' -> '{v}'");
                    if (apply) set(v);
                }
                var addr2 = string.Join(", ", new[] { e.GetValueOrDefault("Address2"), e.GetValueOrDefault("Address3") }.Select(Clean).Where(x => x != null));
                Set("Address 1", e.GetValueOrDefault("Address1"), 200, () => d.Adress1, v => d.Adress1 = v);
                Set("Address 2", addr2, 200, () => d.Adress2, v => d.Adress2 = v);
                Set("Pin", e.GetValueOrDefault("ZipCode"), 50, () => d.Pin, v => d.Pin = v);
                Set("Contact Person", e.GetValueOrDefault("ContactPerson") ?? e.GetValueOrDefault("KeyContactPerson"), 100, () => d.Contactperson, v => d.Contactperson = v);
                Set("Name", e.GetValueOrDefault("CustomerName"), 100, () => d.Compname, v => d.Compname = v, onlyIfBlank: true);
                Set("Mobile", e.GetValueOrDefault("Mobile"), 15, () => d.Mobile, v => d.Mobile = v, onlyIfBlank: true);
                Set("Email", e.GetValueOrDefault("Email"), 100, () => d.Email, v => d.Email = v, onlyIfBlank: true);
                if (changes.Count == 0) { Add(result, "Unchanged", code, d.Compname, null); continue; }
                if (apply) { d.UpdatedBy = Cut(userId, 100); d.UpdatedDate = now; }
                Add(result, "Update", code, d.Compname, string.Join("; ", changes));
            }

            var activeCodes = dealers.Where(x => x.IsActive).Select(x => Code(x.Dealercode)).ToHashSet();
            var allCodes = dealers.Select(x => Code(x.Dealercode)).ToHashSet();
            foreach (var (code, e) in erpBy.OrderBy(k => k.Key))
            {
                if (activeCodes.Contains(code)) continue;
                Add(result, "Skip", code, e.GetValueOrDefault("CustomerName"),
                    allCodes.Contains(code) ? "Active in ERP but Inactive in DMS (UAT report) - not changed" : "In ERP but not in DMS Dealer Master - not added (import it from the UAT report)");
            }

            result.ActiveAfter = dealers.Count(x => x.IsActive);
            result.InactiveAfter = dealers.Count(x => !x.IsActive);
            if (apply) await _db.SaveChangesAsync(ct);
            return result;
        }

        // ===================================================================== locations: UAT list import
        private static int? AreaId(string? area) => Regex.Replace(area ?? "", @"\s", "").ToLowerInvariant() switch
        {
            "showroom" => 1,
            "store" or "workshop" => 2,
            "yard" => 3,
            _ => null
        };

        private static string AreaName(int id) => id switch { 1 => "Showroom", 2 => "Workshop", 3 => "Yard", _ => id.ToString() };

        private sealed class LocRow
        {
            public int Row; public string Code = ""; public string? Name; public string Dealer = ""; public int Area; public bool Active; public int Seen;
        }

        public async Task<ErpSyncResult> ImportUatLocationsAsync(IFormFile file, bool apply, bool inactivateMissing, string userId, CancellationToken ct)
        {
            var result = new ErpSyncResult { Applied = apply, Source = "UAT ERP Location List: " + file?.FileName };
            var sheet = ReadSheet(file!, LocationHeaders, new[] { "Dealer Name", "Source", "SupplierCode" });
            result.TotalRows = sheet.Count;

            var dealers = await _db.DealerMasters.AsNoTracking().ToListAsync(ct);
            var dealerBy = dealers.GroupBy(d => Code(d.Dealercode)).ToDictionary(g => g.Key, g => g.First());

            // 1. sheet rows -> one row per location code (an Active row wins over In Active duplicates)
            var rows = new Dictionary<string, LocRow>(StringComparer.OrdinalIgnoreCase);
            foreach (var (rowNo, v) in sheet)
            {
                var code = Code(Val(v, "Location Code"));
                var dealer = Code(Val(v, "Dealer Code"));
                var name = Val(v, "Location Name");
                var area = AreaId(Val(v, "Location Area"));
                if (code.Length == 0) { Add(result, "Skip", "", name, "Location Code is blank", rowNo, dealer); continue; }
                if (!Regex.IsMatch(dealer, @"^CUS\d+$") || !code.StartsWith(dealer, StringComparison.OrdinalIgnoreCase))
                {
                    Add(result, "Skip", code, name, $"Location Code does not start with its Dealer Code '{dealer}' - correct it in ERP", rowNo, dealer);
                    continue;
                }
                if (area == null) { Add(result, "Skip", code, name, $"Unknown Location Area '{Val(v, "Location Area")}'", rowNo, dealer); continue; }
                if (!dealerBy.ContainsKey(dealer)) { Add(result, "Skip", code, name, $"Dealer {dealer} is not in Dealer Master - import the dealer report first", rowNo, dealer); continue; }

                var active = IsActiveText(Val(v, "Status"));
                if (rows.TryGetValue(code, out var prev))
                {
                    prev.Seen++;
                    if (active && !prev.Active) { prev.Active = true; prev.Name = name ?? prev.Name; prev.Area = area.Value; prev.Row = rowNo; }
                    continue;
                }
                rows[code] = new LocRow { Row = rowNo, Code = code, Name = name, Dealer = dealer, Area = area.Value, Active = active, Seen = 1 };
            }
            foreach (var r in rows.Values.Where(x => x.Seen > 1))
                result.Notes.Add($"Location Code {r.Code} is {r.Seen} times in the file - imported once ({(r.Active ? "Active" : "Inactive")}).");

            // 2. compare with LocationMaster
            var locs = await _db.LocationMasters.ToListAsync(ct);
            var locBy = locs.GroupBy(l => Code(l.Loccode)).ToDictionary(g => g.Key, g => g.First());
            var now = DateTime.Now;
            var toInsert = new List<LocRow>();

            foreach (var r in rows.Values.OrderBy(x => x.Code))
            {
                var flag = r.Active ? "Y" : "N";
                if (!locBy.TryGetValue(r.Code, out var l))
                {
                    toInsert.Add(r);
                    Add(result, "Insert", r.Code, r.Name, $"New {AreaName(r.Area)} ({(r.Active ? "Active" : "Inactive")})", r.Row, r.Dealer);
                    continue;
                }
                var changes = new List<string>();
                if (r.Name != null && !Same(l.Locname, r.Name)) changes.Add($"Name '{l.Locname}' -> '{r.Name}'");
                if (l.Locareaidno != r.Area) changes.Add($"Area {AreaName(l.Locareaidno)} -> {AreaName(r.Area)}");
                if (!Same(l.Dealercode, r.Dealer)) changes.Add($"Dealer {l.Dealercode} -> {r.Dealer}");
                var wasActive = string.Equals(l.Active, "Y", StringComparison.OrdinalIgnoreCase);
                string action = wasActive != r.Active ? (r.Active ? "Activate" : "Inactivate") : (changes.Count > 0 ? "Update" : "Unchanged");
                if (wasActive != r.Active) changes.Insert(0, $"Status {(wasActive ? "Active" : "Inactive")} -> {(r.Active ? "Active" : "Inactive")}");
                Add(result, action, r.Code, r.Name ?? l.Locname, changes.Count > 0 ? string.Join("; ", changes) : null, r.Row, r.Dealer);
                if (apply && action != "Unchanged")
                {
                    if (r.Name != null) l.Locname = Cut(r.Name, 150)!;
                    l.Locareaidno = r.Area;
                    l.Dealercode = r.Dealer;
                    l.Active = flag;
                    l.UpdatedBy = Cut(userId, 50);
                    l.UpdatedDate = now;
                }
            }

            if (inactivateMissing)
            {
                foreach (var l in locs.Where(x => string.Equals(x.Active, "Y", StringComparison.OrdinalIgnoreCase) && !rows.ContainsKey(Code(x.Loccode))).OrderBy(x => x.Loccode))
                {
                    Add(result, "Inactivate", Code(l.Loccode), l.Locname, "Not in the UAT ERP location list", null, l.Dealercode);
                    if (apply) { l.Active = "N"; l.UpdatedBy = Cut(userId, 50); l.UpdatedDate = now; }
                }
            }

            if (apply)
            {
                foreach (var r in toInsert)
                {
                    var d = dealerBy[r.Dealer];
                    _db.LocationMasters.Add(new LocationMaster
                    {
                        Loccode = r.Code,
                        Locname = Cut(r.Name ?? r.Code, 150)!,
                        Locareaidno = r.Area,
                        Add1 = Cut(string.IsNullOrWhiteSpace(d.Adress1) ? (d.City ?? "-") : d.Adress1, 255)!,
                        Add2 = Cut(d.Adress2, 255),
                        City = Cut(d.City ?? "", 100)!,
                        State = Cut(d.State ?? "", 100)!,
                        Pincode = Cut(d.Pin ?? "", 10)!,
                        Mobileno = Cut(d.Mobile ?? "", 15)!,
                        Email = Cut(d.Email, 150),
                        Contpername1 = Cut(string.IsNullOrWhiteSpace(d.Contactperson) ? d.Compname : d.Contactperson, 150) ?? "",
                        Formtype = "DL",
                        Dealercode = r.Dealer,
                        Rrglocationidno = 0,
                        Active = r.Active ? "Y" : "N",
                        CreatedBy = Cut(userId, 50),
                        CreatedDate = now
                    });
                }
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                result.Notes.Add("New locations take the dealer's address / contact for now - run 'Sync from ERP' to fill the ERP address, GSTIN and contacts.");
            }

            var all = apply ? await _db.LocationMasters.AsNoTracking().Select(l => l.Active).ToListAsync(ct) : null;
            if (all != null)
            {
                result.ActiveAfter = all.Count(a => string.Equals(a, "Y", StringComparison.OrdinalIgnoreCase));
                result.InactiveAfter = all.Count - result.ActiveAfter;
            }
            else
            {
                var activeNow = locs.Count(x => string.Equals(x.Active, "Y", StringComparison.OrdinalIgnoreCase));
                result.ActiveAfter = activeNow + result.Activated - result.Inactivated + toInsert.Count(x => x.Active);
                result.InactiveAfter = locs.Count + toInsert.Count - result.ActiveAfter;
            }
            return result;
        }

        // ===================================================================== locations: ERP sync
        public async Task<ErpSyncResult> SyncLocationsFromErpAsync(string? dealerCode, bool apply, string userId, CancellationToken ct)
        {
            var result = new ErpSyncResult { Applied = apply, Source = "ERP C_DMS_DealerConsigneeDetail" };
            var one = Code(dealerCode);
            var sql = "SELECT * FROM dbo.C_DMS_DealerConsigneeDetail" + (one.Length > 0 ? " WHERE Dealer_Code = @code" : "");
            var erp = await ErpRowsAsync(sql, c => { if (one.Length > 0) c.Parameters.Add("@code", SqlDbType.NVarChar, 50).Value = one; }, ct);
            RequireColumns(erp, "C_DMS_DealerConsigneeDetail", "Loc_Idno", "Loc_Code", "Loc_Name", "LocArea_Idno", "Addr1", "City", "State", "PinCode", "Dealer_Code");
            result.TotalRows = erp.Count;

            var q = _db.LocationMasters.AsQueryable();
            if (one.Length > 0) q = q.Where(l => l.Dealercode == one);
            var locs = await q.ToListAsync(ct);
            var locBy = locs.GroupBy(l => Code(l.Loccode)).ToDictionary(g => g.Key, g => g.First());
            var now = DateTime.Now;

            foreach (var e in erp.OrderBy(x => x.GetValueOrDefault("Loc_Code")))
            {
                var code = Code(e.GetValueOrDefault("Loc_Code"));
                if (code.Length == 0) continue;
                var status = e.GetValueOrDefault("Status");
                if (status != null && status != "1" && !IsActiveText(status))
                {
                    Add(result, "Skip", code, e.GetValueOrDefault("Loc_Name"), $"ERP Status {status} (not active) - not synced", null, e.GetValueOrDefault("Dealer_Code"));
                    continue;
                }
                if (!locBy.TryGetValue(code, out var l))
                {
                    Add(result, "Skip", code, e.GetValueOrDefault("Loc_Name"), "In ERP but not in DMS Location Master - import it from the UAT location list", null, e.GetValueOrDefault("Dealer_Code"));
                    continue;
                }
                var changes = new List<string>();
                void Set(string label, string? erpValue, int max, Func<string?> get, Action<string> set, bool onlyIfBlank = false)
                {
                    var v = Cut(Clean(erpValue), max);
                    if (v == null) return;
                    var cur = get();
                    if (onlyIfBlank && !string.IsNullOrWhiteSpace(cur)) return;
                    if (Same(cur, v)) return;
                    changes.Add($"{label} '{cur}' -> '{v}'");
                    if (apply) set(v);
                }
                Set("Address 1", e.GetValueOrDefault("Addr1"), 255, () => l.Add1, v => l.Add1 = v);
                Set("Address 2", e.GetValueOrDefault("Addr2"), 255, () => l.Add2, v => l.Add2 = v);
                Set("City", e.GetValueOrDefault("City"), 100, () => l.City, v => l.City = v);
                Set("State", e.GetValueOrDefault("State"), 100, () => l.State, v => l.State = v);
                Set("Pincode", e.GetValueOrDefault("PinCode"), 10, () => l.Pincode, v => l.Pincode = v);
                Set("GSTIN", e.GetValueOrDefault("LocGSTIN_No"), 20, () => l.Gstinno, v => l.Gstinno = v);
                Set("Email", e.GetValueOrDefault("Email_Id"), 150, () => l.Email, v => l.Email = v);
                Set("Mobile", e.GetValueOrDefault("Mobile_No"), 15, () => l.Mobileno, v => l.Mobileno = v);
                Set("Contact 1", e.GetValueOrDefault("Contact_PN1"), 150, () => l.Contpername1, v => l.Contpername1 = v);
                Set("Contact 2", e.GetValueOrDefault("Contact_PN2"), 150, () => l.Contpername2, v => l.Contpername2 = v);
                Set("Contact 1 mobile", e.GetValueOrDefault("Mobile_No1"), 15, () => l.Contpermob1, v => l.Contpermob1 = v);
                Set("Contact 2 mobile", e.GetValueOrDefault("Mobile_No2"), 15, () => l.Contpermob2, v => l.Contpermob2 = v);
                Set("Contact 1 email", e.GetValueOrDefault("Email_Id1"), 150, () => l.Contperemail1, v => l.Contperemail1 = v);
                Set("Contact 2 email", e.GetValueOrDefault("Email_Id2"), 150, () => l.Contperemail2, v => l.Contperemail2 = v);
                Set("Name", e.GetValueOrDefault("Loc_Name"), 150, () => l.Locname, v => l.Locname = v, onlyIfBlank: true);

                if (int.TryParse(e.GetValueOrDefault("LocArea_Idno"), out var area) && area is >= 1 and <= 3 && l.Locareaidno != area)
                {
                    changes.Add($"Area {AreaName(l.Locareaidno)} -> {AreaName(area)}");
                    if (apply) l.Locareaidno = area;
                }
                if (int.TryParse(e.GetValueOrDefault("Loc_Idno"), out var erpId) && l.Rrglocationidno != erpId)
                {
                    changes.Add($"ERP Loc_Idno {l.Rrglocationidno} -> {erpId}");
                    if (apply) l.Rrglocationidno = erpId;
                }
                if (int.TryParse(e.GetValueOrDefault("LineNo"), out var lineNo) && l.Lineno != lineNo)
                {
                    changes.Add($"LineNo {l.Lineno} -> {lineNo}");
                    if (apply) l.Lineno = lineNo;
                }
                if (changes.Count == 0) { Add(result, "Unchanged", code, l.Locname, null, null, l.Dealercode); continue; }
                if (apply) { l.UpdatedBy = Cut(userId, 50); l.UpdatedDate = now; }
                Add(result, "Update", code, l.Locname, string.Join("; ", changes), null, l.Dealercode);
            }

            var erpCodes = erp.Select(e => Code(e.GetValueOrDefault("Loc_Code"))).ToHashSet();
            foreach (var l in locs.Where(x => string.Equals(x.Active, "Y", StringComparison.OrdinalIgnoreCase) && !erpCodes.Contains(Code(x.Loccode))).OrderBy(x => x.Loccode))
                Add(result, "Skip", Code(l.Loccode), l.Locname, "Active in DMS but not in ERP C_DMS_DealerConsigneeDetail - not changed", null, l.Dealercode);

            result.ActiveAfter = locs.Count(x => string.Equals(x.Active, "Y", StringComparison.OrdinalIgnoreCase));
            result.InactiveAfter = locs.Count - result.ActiveAfter;
            if (apply) await _db.SaveChangesAsync(ct);
            return result;
        }
    }
}
