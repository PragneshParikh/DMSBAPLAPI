using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DMSBAPLAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/zero-hours-job-data")]
    public class ZeroHoursJobDataController : ControllerBase
    {
        private readonly BapldmsvadContext _context;

        public ZeroHoursJobDataController(BapldmsvadContext context)
        {
            _context = context;
        }

        private bool IsSuperAdmin()
        {
            var roles = User.FindAll(ClaimTypes.Role)
                .Concat(User.FindAll("role"))
                .Concat(User.FindAll("Role"))
                .Concat(User.FindAll("RoleName"))
                .Concat(User.FindAll("UserType"))
                .Select(c => c.Value?.Trim())
                .Where(v => !string.IsNullOrEmpty(v));

            return roles.Any(r =>
                r.Contains("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                r.Contains("Admin", StringComparison.OrdinalIgnoreCase));
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

        [HttpPost("import")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Please select an Excel or CSV file." });

                _context.Database.SetCommandTimeout(300);

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext == ".csv")
                    return await ImportCsvAsync(file);

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();
                var headerRow = worksheet.FirstRowUsed();
                if (headerRow == null)
                    return BadRequest(new { message = "Sheet is empty." });

                var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in headerRow.CellsUsed())
                {
                    var h = cell.GetString().Trim();
                    if (!string.IsNullOrEmpty(h) && !colMap.ContainsKey(h))
                        colMap[h] = cell.Address.ColumnNumber;
                }

                var createdBy = GetCurrentUserName();
                var now = DateTime.Now;
                var batch = new List<DmsZeroHoursJobData>(5000);
                int inserted = 0;

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var dealerCode = GetCellStr(row, colMap, "Dealer Code");
                    var jobNo = GetCellStr(row, colMap, "Job No");
                    if (string.IsNullOrWhiteSpace(dealerCode) && string.IsNullOrWhiteSpace(jobNo))
                        continue;

                    batch.Add(MapFromExcelRow(row, colMap, createdBy, now));
                    if (batch.Count >= 5000)
                    {
                        await BulkInsertZeroHoursAsync(batch);
                        inserted += batch.Count;
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    await BulkInsertZeroHoursAsync(batch);
                    inserted += batch.Count;
                    batch.Clear();
                }

                return Ok(new { message = $"Import successful. Inserted: {inserted}", inserted });
            }
            catch (Exception ex)
            {
                var baseEx = ex.GetBaseException();
                return StatusCode(500, new { message = baseEx.Message, detail = ex.Message });
            }
        }

        private async Task<IActionResult> ImportCsvAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                var headerLine = await ReadNextCsvRecordAsync(reader);
                if (string.IsNullOrWhiteSpace(headerLine))
                    return BadRequest(new { message = "CSV is empty." });

                var headers = SplitCsvLine(headerLine);
                var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < headers.Count; i++)
                {
                    var h = headers[i].Trim();
                    if (!string.IsNullOrEmpty(h) && !colMap.ContainsKey(h))
                        colMap[h] = i;
                }

                var createdBy = GetCurrentUserName();
                var now = DateTime.Now;
                var batch = new List<DmsZeroHoursJobData>(5000);
                int inserted = 0;

                string? record;
                while ((record = await ReadNextCsvRecordAsync(reader)) != null)
                {
                    if (string.IsNullOrWhiteSpace(record)) continue;
                    var vals = SplitCsvLine(record);
                    var entity = MapFromCsvRow(vals, colMap, createdBy, now);

                    if (string.IsNullOrWhiteSpace(entity.DealerCode) && string.IsNullOrWhiteSpace(entity.JobNo))
                        continue;

                    batch.Add(entity);
                    if (batch.Count >= 5000)
                    {
                        await BulkInsertZeroHoursAsync(batch);
                        inserted += batch.Count;
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    await BulkInsertZeroHoursAsync(batch);
                    inserted += batch.Count;
                    batch.Clear();
                }

                return Ok(new { message = $"Import successful. Inserted: {inserted}", inserted });
            }
            catch (Exception ex)
            {
                var baseEx = ex.GetBaseException();
                return StatusCode(500, new { message = baseEx.Message, detail = ex.Message });
            }
        }

        private async Task BulkInsertZeroHoursAsync(List<DmsZeroHoursJobData> rows)
        {
            if (rows.Count == 0) return;

            var table = new DataTable();
            table.Columns.Add("CompName", typeof(string));
            table.Columns.Add("DealerCode", typeof(string));
            table.Columns.Add("DealerState", typeof(string));
            table.Columns.Add("JobDate", typeof(DateTime));
            table.Columns.Add("JobNo", typeof(string));
            table.Columns.Add("JobCategory", typeof(string));
            table.Columns.Add("FfrPer", typeof(decimal));
            table.Columns.Add("DocNo", typeof(string));
            table.Columns.Add("DocDate", typeof(DateTime));
            table.Columns.Add("DocType", typeof(string));
            table.Columns.Add("JobStatus", typeof(string));
            table.Columns.Add("Location", typeof(string));
            table.Columns.Add("Model", typeof(string));
            table.Columns.Add("BrandName", typeof(string));
            table.Columns.Add("PartyName", typeof(string));
            table.Columns.Add("PartyMobile", typeof(string));
            table.Columns.Add("Supervisor", typeof(string));
            table.Columns.Add("Technician", typeof(string));
            table.Columns.Add("Kms", typeof(decimal));
            table.Columns.Add("ServiceHead", typeof(string));
            table.Columns.Add("JobType", typeof(string));
            table.Columns.Add("RegNo", typeof(string));
            table.Columns.Add("VehicleType", typeof(string));
            table.Columns.Add("EngineNo", typeof(string));
            table.Columns.Add("ChassisNo", typeof(string));
            table.Columns.Add("ExpectedDeliveryDate", typeof(DateTime));
            table.Columns.Add("ProformaDate", typeof(DateTime));
            table.Columns.Add("InvoiceDate", typeof(DateTime));
            table.Columns.Add("EstimatedJobExpenses", typeof(decimal));
            table.Columns.Add("LabourHours", typeof(decimal));
            table.Columns.Add("Parts", typeof(decimal));
            table.Columns.Add("Accessory", typeof(decimal));
            table.Columns.Add("Oil", typeof(decimal));
            table.Columns.Add("Labour", typeof(decimal));
            table.Columns.Add("OutsideWork", typeof(decimal));
            table.Columns.Add("TotalWoTax", typeof(decimal));
            table.Columns.Add("GstAmnt", typeof(decimal));
            table.Columns.Add("IgstAmnt", typeof(decimal));
            table.Columns.Add("NetTotal", typeof(decimal));
            table.Columns.Add("InTime", typeof(string));
            table.Columns.Add("CloseTime", typeof(string));
            table.Columns.Add("CloseDate", typeof(DateTime));
            table.Columns.Add("CopnNo", typeof(string));
            table.Columns.Add("SaleDate", typeof(DateTime));
            table.Columns.Add("BatterySerialNo1", typeof(string));
            table.Columns.Add("BatterySerialNo2", typeof(string));
            table.Columns.Add("BatterySerialNo3", typeof(string));
            table.Columns.Add("BatterySerialNo4", typeof(string));
            table.Columns.Add("BatterySerialNo5", typeof(string));
            table.Columns.Add("BatterySerialNo6", typeof(string));
            table.Columns.Add("IndividualAhBattery1", typeof(string));
            table.Columns.Add("IndividualAhBattery2", typeof(string));
            table.Columns.Add("IndividualAhBattery3", typeof(string));
            table.Columns.Add("IndividualAhBattery4", typeof(string));
            table.Columns.Add("IndividualAhBattery5", typeof(string));
            table.Columns.Add("IndividualAhBattery6", typeof(string));
            table.Columns.Add("ChargerNo", typeof(string));
            table.Columns.Add("ControllerNo", typeof(string));
            table.Columns.Add("Converter", typeof(string));
            table.Columns.Add("CreateThrough", typeof(string));
            table.Columns.Add("ZoneName", typeof(string));
            table.Columns.Add("UniqueId", typeof(string));
            table.Columns.Add("DealerType", typeof(string));
            table.Columns.Add("CreatedDate", typeof(DateTime));
            table.Columns.Add("CreatedBy", typeof(string));

            static object DbDate(DateOnly? v) => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
            static object DbDec(decimal? v) =>
                v.HasValue && Math.Abs(v.Value) <= 999999999999999m ? v.Value : DBNull.Value;
            static object DbStr(string? v) => string.IsNullOrWhiteSpace(v) ? DBNull.Value : v;

            foreach (var r in rows)
            {
                table.Rows.Add(
                    DbStr(r.CompName), DbStr(r.DealerCode), DbStr(r.DealerState), DbDate(r.JobDate),
                    DbStr(r.JobNo), DbStr(r.JobCategory), DbDec(r.FfrPer), DbStr(r.DocNo),
                    DbDate(r.DocDate), DbStr(r.DocType), DbStr(r.JobStatus), DbStr(r.Location),
                    DbStr(r.Model), DbStr(r.BrandName), DbStr(r.PartyName), DbStr(r.PartyMobile),
                    DbStr(r.Supervisor), DbStr(r.Technician), DbDec(r.Kms), DbStr(r.ServiceHead),
                    DbStr(r.JobType), DbStr(r.RegNo), DbStr(r.VehicleType), DbStr(r.EngineNo),
                    DbStr(r.ChassisNo), DbDate(r.ExpectedDeliveryDate), DbDate(r.ProformaDate),
                    DbDate(r.InvoiceDate), DbDec(r.EstimatedJobExpenses), DbDec(r.LabourHours),
                    DbDec(r.Parts), DbDec(r.Accessory), DbDec(r.Oil), DbDec(r.Labour),
                    DbDec(r.OutsideWork), DbDec(r.TotalWoTax), DbDec(r.GstAmnt), DbDec(r.IgstAmnt),
                    DbDec(r.NetTotal), DbStr(r.InTime), DbStr(r.CloseTime), DbDate(r.CloseDate),
                    DbStr(r.CopnNo), DbDate(r.SaleDate), DbStr(r.BatterySerialNo1),
                    DbStr(r.BatterySerialNo2), DbStr(r.BatterySerialNo3), DbStr(r.BatterySerialNo4),
                    DbStr(r.BatterySerialNo5), DbStr(r.BatterySerialNo6), DbStr(r.IndividualAhBattery1),
                    DbStr(r.IndividualAhBattery2), DbStr(r.IndividualAhBattery3),
                    DbStr(r.IndividualAhBattery4), DbStr(r.IndividualAhBattery5),
                    DbStr(r.IndividualAhBattery6), DbStr(r.ChargerNo), DbStr(r.ControllerNo),
                    DbStr(r.Converter), DbStr(r.CreateThrough), DbStr(r.ZoneName),
                    DbStr(r.UniqueId), DbStr(r.DealerType),
                    r.CreatedDate.HasValue ? r.CreatedDate.Value : DBNull.Value,
                    DbStr(r.CreatedBy)
                );
            }

            var connString = _context.Database.GetConnectionString();
            using var bulk = new SqlBulkCopy(connString)
            {
                DestinationTableName = "dbo.DMS_ZeroHoursJobData",
                BatchSize = 5000,
                BulkCopyTimeout = 300
            };

            foreach (DataColumn col in table.Columns)
                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

            await bulk.WriteToServerAsync(table);
        }

        //[HttpGet("list")]
        //public async Task<IActionResult> List(
        //    [FromQuery] string? DealerCode,
        //    [FromQuery] DateTime? FromDate,
        //    [FromQuery] DateTime? ToDate,
        //    [FromQuery] string? Search,
        //    [FromQuery] int PageIndex = 1,
        //    [FromQuery] int PageSize = 25)
        //{
        //    try
        //    {
        //        var query = BuildFilteredQuery(DealerCode, FromDate, ToDate, Search, out var unauthorizedMsg);
        //        if (unauthorizedMsg != null)
        //            return Unauthorized(unauthorizedMsg);

        //        var totalRecords = await query.CountAsync();
        //        PageIndex = Math.Max(1, PageIndex);
        //        PageSize = Math.Clamp(PageSize, 1, 500);

        //        var data = await query
        //            .OrderByDescending(x => x.JobDate)
        //            .ThenByDescending(x => x.Id)
        //            .Skip((PageIndex - 1) * PageSize)
        //            .Take(PageSize)
        //            .ToListAsync();

        //        return Ok(new { data, totalRecords });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.GetBaseException().Message });
        //    }
        //}

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
                _context.Database.SetCommandTimeout(180);

                var query = BuildFilteredQuery(DealerCode, FromDate, ToDate, Search, out var unauthorizedMsg);
                if (unauthorizedMsg != null)
                    return Unauthorized(unauthorizedMsg);

                PageIndex = Math.Max(1, PageIndex);
                PageSize = Math.Clamp(PageSize, 1, 500);

                var totalRecords = await query.CountAsync();

                var data = await query
                    .OrderBy(x => x.JobDate)
                    .ThenBy(x => x.Id)
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
                    .OrderByDescending(x => x.JobDate)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("ZeroHoursJobData");

                var headers = GetAllCsvHeaders();
                for (int c = 0; c < headers.Length; c++)
                {
                    ws.Cell(1, c + 1).Value = headers[c];
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                }

                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    int rIdx = i + 2;
                    WriteExportRow(ws, rIdx, r);
                }

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return File(ms.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"ZeroHoursJobData_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        private IQueryable<DmsZeroHoursJobData> BuildFilteredQuery(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate,
            string? search,
            out string? unauthorizedMsg)
        {
            unauthorizedMsg = null;
            var query = _context.DmsZeroHoursJobData.AsNoTracking().AsQueryable();

            if (IsSuperAdmin())
            {
                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    var dc = dealerCode.Trim();
                    query = query.Where(x => x.DealerCode == dc);
                }
            }
            else
            {
                var userDealer = GetUserDealerCode();
                if (string.IsNullOrWhiteSpace(userDealer))
                {
                    unauthorizedMsg = "Dealer code not found for user";
                    return query.Where(_ => false);
                }
                query = query.Where(x => x.DealerCode == userDealer);
            }

            if (fromDate.HasValue)
            {
                var from = DateOnly.FromDateTime(fromDate.Value);
                query = query.Where(x => x.JobDate >= from);
            }

            if (toDate.HasValue)
            {
                var to = DateOnly.FromDateTime(toDate.Value);
                query = query.Where(x => x.JobDate <= to);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    (x.JobNo != null && x.JobNo.Contains(s)) ||
                    (x.ChassisNo != null && x.ChassisNo.Contains(s)) ||
                    (x.PartyName != null && x.PartyName.Contains(s)) ||
                    (x.PartyMobile != null && x.PartyMobile.Contains(s)) ||
                    (x.Model != null && x.Model.Contains(s)) ||
                    (x.DealerCode != null && x.DealerCode.Contains(s)));
            }

            return query;
        }

        private static DmsZeroHoursJobData MapFromExcelRow(IXLRow row, Dictionary<string, int> map, string createdBy, DateTime now)
        {
            string? S(string col) => map.TryGetValue(col, out var idx) ? NullIfEmpty(row.Cell(idx).GetFormattedString()?.Trim()) : null;
            DateOnly? D(string col) => ParseDate(S(col));
            decimal? Dec(string col) => decimal.TryParse(S(col), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

            return BuildEntity(S, D, Dec, createdBy, now);
        }

        private static DmsZeroHoursJobData MapFromCsvRow(List<string> vals, Dictionary<string, int> map, string createdBy, DateTime now)
        {
            string? S(string col) => map.TryGetValue(col, out var idx) && idx < vals.Count ? NullIfEmpty(vals[idx].Trim()) : null;
            DateOnly? D(string col) => ParseDate(S(col));
            decimal? Dec(string col) => decimal.TryParse(S(col), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

            return BuildEntity(S, D, Dec, createdBy, now);
        }

        private static DmsZeroHoursJobData BuildEntity(
            Func<string, string?> S,
            Func<string, DateOnly?> D,
            Func<string, decimal?> Dec,
            string createdBy,
            DateTime now) => new()
            {
                CompName = S("Comp Name"),
                DealerCode = S("Dealer Code"),
                DealerState = S("Dealer State"),
                JobDate = D("Job Date"),
                JobNo = S("Job No"),
                JobCategory = S("Job Category"),
                FfrPer = Dec("FFR Per%"),
                DocNo = S("Doc No"),
                DocDate = D("Doc Date"),
                DocType = S("Doc Type"),
                JobStatus = S("Job Status"),
                Location = S("Location"),
                Model = S("Model"),
                BrandName = S("Brand Name"),
                PartyName = S("Party Name"),
                PartyMobile = S("Party Mobile"),
                Supervisor = S("Supervisor"),
                Technician = S("Technician"),
                Kms = Dec("KMS"),
                ServiceHead = S("Service Head"),
                JobType = S("Job Type"),
                RegNo = S("Reg No"),
                VehicleType = S("Vehicle Type"),
                EngineNo = S("Engine No"),
                ChassisNo = S("Chassis No"),
                ExpectedDeliveryDate = D("Expected delivery date"),
                ProformaDate = D("Proforma Date"),
                InvoiceDate = D("Invoice Date"),
                EstimatedJobExpenses = Dec("Estimated Job expenses"),
                LabourHours = Dec("Labour hours"),
                Parts = Dec("Parts"),
                Accessory = Dec("Accessory"),
                Oil = Dec("Oil"),
                Labour = Dec("Labour"),
                OutsideWork = Dec("Outside Work"),
                TotalWoTax = Dec("Total W/O Tax"),
                GstAmnt = Dec("GST_Amnt"),
                IgstAmnt = Dec("IGST_Amnt"),
                NetTotal = Dec("Net Total"),
                InTime = S("In Time"),
                CloseTime = S("Close Time"),
                CloseDate = D("Close Date"),
                CopnNo = S("Copn_No"),
                SaleDate = D("sale_date"),
                BatterySerialNo1 = S("Battery_SerialNo1"),
                BatterySerialNo2 = S("Battery_SerialNo2"),
                BatterySerialNo3 = S("Battery_SerialNo3"),
                BatterySerialNo4 = S("Battery_SerialNo4"),
                BatterySerialNo5 = S("Battery_SerialNo5"),
                BatterySerialNo6 = S("Battery_SerialNo6"),
                IndividualAhBattery1 = S("IndividualAH_Battery1"),
                IndividualAhBattery2 = S("IndividualAH_Battery2"),
                IndividualAhBattery3 = S("IndividualAH_Battery3"),
                IndividualAhBattery4 = S("IndividualAH_Battery4"),
                IndividualAhBattery5 = S("IndividualAH_Battery5"),
                IndividualAhBattery6 = S("IndividualAH_Battery6"),
                ChargerNo = S("Charger No"),
                ControllerNo = S("Controller No"),
                Converter = S("Converter"),
                CreateThrough = S("Create Through"),
                ZoneName = S("Zone Name"),
                UniqueId = S("UniqueId"),
                DealerType = S("Dealer Type"),
                CreatedDate = now,
                CreatedBy = createdBy
            };

        private static string? GetCellStr(IXLRow row, Dictionary<string, int> map, string col) =>
            map.TryGetValue(col, out var idx) ? NullIfEmpty(row.Cell(idx).GetFormattedString()?.Trim()) : null;

        private static string? NullIfEmpty(string? val) => string.IsNullOrWhiteSpace(val) ? null : val;

        private static DateOnly? ParseDate(string? str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            string[] formats = { "dd-MM-yyyy", "d-M-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
            return DateOnly.TryParseExact(str.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
        }

        private static async Task<string?> ReadNextCsvRecordAsync(StreamReader reader)
        {
            string? line = await reader.ReadLineAsync();
            if (line == null) return null;

            var sb = new StringBuilder(line);
            while (CountQuotes(sb.ToString()) % 2 != 0)
            {
                var next = await reader.ReadLineAsync();
                if (next == null) break;
                sb.Append('\n').Append(next);
            }
            return sb.ToString();
        }

        private static int CountQuotes(string s)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
                if (s[i] == '"') count++;
            return count;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var cur = new StringBuilder();
            foreach (char c in line)
            {
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes) { result.Add(cur.ToString()); cur.Clear(); }
                else cur.Append(c);
            }
            result.Add(cur.ToString());
            return result;
        }

        private static string[] GetAllCsvHeaders() => new[]
        {
            "Comp Name", "Dealer Code", "Dealer State", "Job Date", "Job No", "Job Category",
            "FFR Per%", "Doc No", "Doc Date", "Doc Type", "Job Status", "Location", "Model",
            "Brand Name", "Party Name", "Party Mobile", "Supervisor", "Technician", "KMS",
            "Service Head", "Job Type", "Reg No", "Vehicle Type", "Engine No", "Chassis No",
            "Expected delivery date", "Proforma Date", "Invoice Date", "Estimated Job expenses",
            "Labour hours", "Parts", "Accessory", "Oil", "Labour", "Outside Work", "Total W/O Tax",
            "GST_Amnt", "IGST_Amnt", "Net Total", "In Time", "Close Time", "Close Date",
            "Copn_No", "sale_date", "Battery_SerialNo1", "Battery_SerialNo2", "Battery_SerialNo3",
            "Battery_SerialNo4", "BatterySerialNo5", "Battery_SerialNo6", "IndividualAH_Battery1",
            "IndividualAH_Battery2", "IndividualAH_Battery3", "IndividualAH_Battery4",
            "IndividualAH_Battery5", "IndividualAH_Battery6", "Charger No", "Controller No",
            "Converter", "Create Through", "Zone Name", "UniqueId", "Dealer Type"
        };

        private static void WriteExportRow(IXLWorksheet ws, int rIdx, DmsZeroHoursJobData r)
        {
            object?[] vals =
            {
                r.CompName, r.DealerCode, r.DealerState, r.JobDate?.ToString("dd-MM-yyyy"), r.JobNo,
                r.JobCategory, r.FfrPer, r.DocNo, r.DocDate?.ToString("dd-MM-yyyy"), r.DocType,
                r.JobStatus, r.Location, r.Model, r.BrandName, r.PartyName, r.PartyMobile,
                r.Supervisor, r.Technician, r.Kms, r.ServiceHead, r.JobType, r.RegNo, r.VehicleType,
                r.EngineNo, r.ChassisNo, r.ExpectedDeliveryDate?.ToString("dd-MM-yyyy"),
                r.ProformaDate?.ToString("dd-MM-yyyy"), r.InvoiceDate?.ToString("dd-MM-yyyy"),
                r.EstimatedJobExpenses, r.LabourHours, r.Parts, r.Accessory, r.Oil, r.Labour,
                r.OutsideWork, r.TotalWoTax, r.GstAmnt, r.IgstAmnt, r.NetTotal, r.InTime, r.CloseTime,
                r.CloseDate?.ToString("dd-MM-yyyy"), r.CopnNo, r.SaleDate?.ToString("dd-MM-yyyy"),
                r.BatterySerialNo1, r.BatterySerialNo2, r.BatterySerialNo3, r.BatterySerialNo4,
                r.BatterySerialNo5, r.BatterySerialNo6, r.IndividualAhBattery1, r.IndividualAhBattery2,
                r.IndividualAhBattery3, r.IndividualAhBattery4, r.IndividualAhBattery5,
                r.IndividualAhBattery6, r.ChargerNo, r.ControllerNo, r.Converter, r.CreateThrough,
                r.ZoneName, r.UniqueId, r.DealerType
            };
            for (int c = 0; c < vals.Length; c++)
                ws.Cell(rIdx, c + 1).Value = vals[c]?.ToString() ?? "";
        }
    }
}