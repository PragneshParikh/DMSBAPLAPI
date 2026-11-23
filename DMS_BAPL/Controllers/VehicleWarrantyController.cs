using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMSBAPLAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/vehicle-warranty")]
    public class VehicleWarrantyController : ControllerBase
    {
        private readonly BapldmsvadContext _context;

        public VehicleWarrantyController(BapldmsvadContext context)
        {
            _context = context;
        }

        private bool IsSuperAdmin()
        {
            var roles = User.FindAll(ClaimTypes.Role)
                .Concat(User.FindAll("role"))
                .Select(c => c.Value?.Trim())
                .Where(v => !string.IsNullOrEmpty(v));

            return roles.Any(r =>
                string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));
        }

        private string? GetUserDealerCode()
        {
            return User.FindFirst("DealerCode")?.Value?.Trim()
                ?? User.FindFirst("dealerCode")?.Value?.Trim();
        }

        private string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("UserName")?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? "System";
        }

        private async Task<Dictionary<string, string>> BuildDealerLookupAsync()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var dealers = await _context.DealerMasters.AsNoTracking().ToListAsync();
            if (dealers.Count == 0) return lookup;

            var props = typeof(DealerMaster).GetProperties();
            var nameProp = props.FirstOrDefault(p =>
                p.Name.Equals("DealerName", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("CompanyName", StringComparison.OrdinalIgnoreCase));

            var codeProp = props.FirstOrDefault(p =>
                p.Name.Equals("DealerCode", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("Code", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("CustomerCode", StringComparison.OrdinalIgnoreCase));

            if (nameProp == null || codeProp == null) return lookup;

            foreach (var d in dealers)
            {
                var name = nameProp.GetValue(d)?.ToString()?.Trim();
                var code = codeProp.GetValue(d)?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(code) && !lookup.ContainsKey(name))
                {
                    lookup[name] = code;
                }
            }

            return lookup;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Please select an Excel file." });

                var dealerLookup = await BuildDealerLookupAsync();

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault(w =>
                    string.Equals(w.Name.Trim(), "WarrantyReport", StringComparison.OrdinalIgnoreCase))
                    ?? workbook.Worksheets.First();

                var headerRow = worksheet.FirstRowUsed();
                if (headerRow == null)
                    return BadRequest(new { message = "Excel sheet is empty." });

                var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in headerRow.CellsUsed())
                {
                    var header = cell.GetString().Trim();
                    if (!string.IsNullOrEmpty(header) && !colMap.ContainsKey(header))
                        colMap[header] = cell.Address.ColumnNumber;
                }

                if (!colMap.ContainsKey("Chasis No"))
                    return BadRequest(new { message = "Invalid Excel format: 'Chasis No' column is missing." });

                var existingChassis = await _context.DmsVehicleWarranties
                    .ToDictionaryAsync(x => x.ChasisNo.Trim().ToUpper());

                var createdBy = GetCurrentUserName();
                var now = DateTime.Now;
                int inserted = 0, updated = 0;

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var chassis = GetStr(row, colMap, "Chasis No");
                    if (string.IsNullOrWhiteSpace(chassis))
                        continue;

                    var dealerName = GetStr(row, colMap, "Dealer Name");
                    string? dealerCode = null;
                    if (!string.IsNullOrWhiteSpace(dealerName) &&
                        dealerLookup.TryGetValue(dealerName.Trim(), out var mappedCode))
                    {
                        dealerCode = mappedCode;
                    }

                    var key = chassis.Trim().ToUpper();
                    if (!existingChassis.TryGetValue(key, out var entity))
                    {
                        entity = new DmsVehicleWarranty
                        {
                            ChasisNo = chassis.Trim(),
                            CreatedDate = now,
                            CreatedBy = createdBy
                        };
                        _context.DmsVehicleWarranties.Add(entity);
                        existingChassis[key] = entity;
                        inserted++;
                    }
                    else
                    {
                        updated++;
                    }

                    entity.DealerName = dealerName;
                    entity.DealerCode = dealerCode;
                    entity.ItemCode = GetStr(row, colMap, "Item Code");
                    entity.DisplayName = GetStr(row, colMap, "Display Name");
                    entity.City = GetStr(row, colMap, "City");
                    entity.State = GetStr(row, colMap, "State");
                    entity.ItemName = GetStr(row, colMap, "Item Name");
                    entity.ItemDescription = GetStr(row, colMap, "Item Description");
                    entity.CustomerName = GetStr(row, colMap, "Customer Name");
                    entity.MobileNo = GetStr(row, colMap, "Mobile No.");
                    entity.DateOfSales = GetDateOnly(row, colMap, "Date of Sales");
                    entity.DeviceGroup = GetStr(row, colMap, "Device Group");
                    entity.DeviceType = GetStr(row, colMap, "Device Type");
                    entity.VehicleStandardWarrantyMonths = GetInt(row, colMap, "Vehicle Standard Warranty (Months)");
                    entity.VehicleStandardWarrantyOdo = GetInt(row, colMap, "Vehicle Standard Warranty (ODO Reading)");
                    entity.VehicleExtWarrantyScheme = GetStr(row, colMap, "Vehicle Ext. Warranty Scheme");
                    entity.VehicleExtendedWarrantyMonths = GetInt(row, colMap, "Vehicle Extended Warranty (Months)");
                    entity.VehicleExtendedWarrantyOdo = GetDecimal(row, colMap, "Vehicle Extended Warranty (ODO Reading)");
                    entity.DateOfStandardWarrantyExpiry = GetDateOnly(row, colMap, "Date of Standard Warranty Expiry (as per Date)");
                    entity.DateOfExtendedWarrantyExpiry = GetDateOnly(row, colMap, "Date of Extended Warranty Expiry (as per Date)");
                    entity.LastOdoReadingDate = GetDateOnly(row, colMap, "Last ODO Reading Date");
                    entity.OdoReadingAsPerLastDate = GetInt(row, colMap, "ODO Reading as per last date");
                    entity.CurrentWarrantyStatusAsPerDate = GetStr(row, colMap, "Current Warranty Status (As per date)");
                    entity.CurrentWarrantyStatusAsPerOdo = GetStr(row, colMap, "Current Warranty Status (As per ODO Reading)");
                    entity.FinalWarrantyStatus = GetStr(row, colMap, "Final Warranty Status");
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Import successful. Inserted: {inserted}, Updated: {updated}",
                    inserted,
                    updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> List(
            [FromQuery] string? DealerCode,
            [FromQuery] DateTime? FromDate,
            [FromQuery] DateTime? ToDate,
            [FromQuery] string? Search,
            [FromQuery] int PageIndex = 1,
            [FromQuery] int PageSize = 25)
        {
            try
            {
                var query = BuildFilteredQuery(DealerCode, FromDate, ToDate, Search, out var unauthorizedMsg);
                if (unauthorizedMsg != null)
                    return Unauthorized(unauthorizedMsg);

                var totalRecords = await query.CountAsync();

                PageIndex = Math.Max(1, PageIndex);
                PageSize = Math.Clamp(PageSize, 1, 500);

                var data = await query
                    .OrderByDescending(x => x.DateOfSales)
                    .ThenByDescending(x => x.Id)
                    .Skip((PageIndex - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                return Ok(new { data, totalRecords });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(
            [FromQuery] string? DealerCode,
            [FromQuery] DateTime? FromDate,
            [FromQuery] DateTime? ToDate,
            [FromQuery] string? Search)
        {
            try
            {
                var query = BuildFilteredQuery(DealerCode, FromDate, ToDate, Search, out var unauthorizedMsg);
                if (unauthorizedMsg != null)
                    return Unauthorized(unauthorizedMsg);

                var rows = await query
                    .OrderByDescending(x => x.DateOfSales)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("WarrantyReport");

                var headers = new[]
                {
                    "Dealer Name", "Chasis No", "Item Code", "Display Name", "City", "State",
                    "Item Name", "Item Description", "Customer Name", "Mobile No.", "Date of Sales",
                    "Device Group", "Device Type", "Vehicle Standard Warranty (Months)",
                    "Vehicle Standard Warranty (ODO Reading)", "Vehicle Ext. Warranty Scheme",
                    "Vehicle Extended Warranty (Months)", "Vehicle Extended Warranty (ODO Reading)",
                    "Date of Standard Warranty Expiry (as per Date)",
                    "Date of Extended Warranty Expiry (as per Date)",
                    "Last ODO Reading Date", "ODO Reading as per last date",
                    "Current Warranty Status (As per date)",
                    "Current Warranty Status (As per ODO Reading)", "Final Warranty Status"
                };

                for (int c = 0; c < headers.Length; c++)
                {
                    ws.Cell(1, c + 1).Value = headers[c];
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                }

                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    int rowIdx = i + 2;
                    ws.Cell(rowIdx, 1).Value = r.DealerName ?? "";
                    ws.Cell(rowIdx, 2).Value = r.ChasisNo;
                    ws.Cell(rowIdx, 3).Value = r.ItemCode ?? "";
                    ws.Cell(rowIdx, 4).Value = r.DisplayName ?? "";
                    ws.Cell(rowIdx, 5).Value = r.City ?? "";
                    ws.Cell(rowIdx, 6).Value = r.State ?? "";
                    ws.Cell(rowIdx, 7).Value = r.ItemName ?? "";
                    ws.Cell(rowIdx, 8).Value = r.ItemDescription ?? "";
                    ws.Cell(rowIdx, 9).Value = r.CustomerName ?? "";
                    ws.Cell(rowIdx, 10).Value = r.MobileNo ?? "";
                    ws.Cell(rowIdx, 11).Value = r.DateOfSales?.ToString("dd-MM-yyyy") ?? "";
                    ws.Cell(rowIdx, 12).Value = r.DeviceGroup ?? "";
                    ws.Cell(rowIdx, 13).Value = r.DeviceType ?? "";
                    ws.Cell(rowIdx, 14).Value = r.VehicleStandardWarrantyMonths ?? 0;
                    ws.Cell(rowIdx, 15).Value = r.VehicleStandardWarrantyOdo ?? 0;
                    ws.Cell(rowIdx, 16).Value = r.VehicleExtWarrantyScheme ?? "";
                    ws.Cell(rowIdx, 17).Value = r.VehicleExtendedWarrantyMonths ?? 0;
                    ws.Cell(rowIdx, 18).Value = r.VehicleExtendedWarrantyOdo ?? 0;
                    ws.Cell(rowIdx, 19).Value = r.DateOfStandardWarrantyExpiry?.ToString("dd-MM-yyyy") ?? "";
                    ws.Cell(rowIdx, 20).Value = r.DateOfExtendedWarrantyExpiry?.ToString("dd-MM-yyyy") ?? "";
                    ws.Cell(rowIdx, 21).Value = r.LastOdoReadingDate?.ToString("dd-MM-yyyy") ?? "";
                    ws.Cell(rowIdx, 22).Value = r.OdoReadingAsPerLastDate ?? 0;
                    ws.Cell(rowIdx, 23).Value = r.CurrentWarrantyStatusAsPerDate ?? "";
                    ws.Cell(rowIdx, 24).Value = r.CurrentWarrantyStatusAsPerOdo ?? "";
                    ws.Cell(rowIdx, 25).Value = r.FinalWarrantyStatus ?? "";
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                var fileName = $"VehicleWarranty_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                return File(ms.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        private IQueryable<DmsVehicleWarranty> BuildFilteredQuery(
             string? dealerCode,
             DateTime? fromDate,
             DateTime? toDate,
             string? search,
             out string? unauthorizedMsg)
        { 
            unauthorizedMsg = null;
            var query = _context.DmsVehicleWarranties.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(dealerCode))
            {
                var dc = dealerCode.Trim();
                query = query.Where(x => x.DealerCode == dc || x.DealerName == dc);
            }

            if (FromDateOnly(fromDate) is DateOnly from)
                query = query.Where(x => x.DateOfSales >= from);

            if (FromDateOnly(toDate) is DateOnly to)
                query = query.Where(x => x.DateOfSales <= to);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    (x.ChasisNo != null && x.ChasisNo.Contains(s)) ||
                    (x.CustomerName != null && x.CustomerName.Contains(s)) ||
                    (x.MobileNo != null && x.MobileNo.Contains(s)) ||
                    (x.DisplayName != null && x.DisplayName.Contains(s)) ||
                    (x.DealerName != null && x.DealerName.Contains(s)));
            }

            return query;
        }
        private static DateOnly? FromDateOnly(DateTime? dt) =>
            dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null;

        private static string? GetStr(IXLRow row, Dictionary<string, int> map, string col)
        {
            if (!map.TryGetValue(col, out int idx)) return null;
            var val = row.Cell(idx).GetFormattedString()?.Trim();
            return string.IsNullOrWhiteSpace(val) ? null : val;
        }

        private static int? GetInt(IXLRow row, Dictionary<string, int> map, string col)
        {
            if (!map.TryGetValue(col, out int idx)) return null;
            var cell = row.Cell(idx);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<double>(out var d)) return (int)Math.Round(d);
            return int.TryParse(cell.GetFormattedString().Trim(), out var v) ? v : null;
        }

        private static decimal? GetDecimal(IXLRow row, Dictionary<string, int> map, string col)
        {
            if (!map.TryGetValue(col, out int idx)) return null;
            var cell = row.Cell(idx);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<double>(out var d)) return (decimal)d;
            return decimal.TryParse(cell.GetFormattedString().Trim(), out var v) ? v : null;
        }

        private static DateOnly? GetDateOnly(IXLRow row, Dictionary<string, int> map, string col)
        {
            if (!map.TryGetValue(col, out int idx)) return null;
            var cell = row.Cell(idx);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<DateTime>(out var dt)) return DateOnly.FromDateTime(dt);

            var str = cell.GetFormattedString().Trim();
            string[] formats = { "dd-MM-yyyy", "d-M-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
            if (DateOnly.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            return null;
        }
    }
}