using System.Security.Claims;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/historical-repair-bill")]
    [ApiController]
    public class HistoricalRepairBillController : ControllerBase
    {
        private readonly BapldmsvadContext _context;
        private readonly ILogger<HistoricalRepairBillController> _logger;

        public HistoricalRepairBillController(
            BapldmsvadContext context,
            ILogger<HistoricalRepairBillController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsAdmin()
        {
            var adminRoles = new[] { "superadmin", "admin", "administrator" };
            return User?.Claims.Any(c => c.Type == ClaimTypes.Role &&
                       adminRoles.Contains(c.Value.Trim().ToLower())) == true;
        }

        private static string? ResolveJobNo(
            List<(DateOnly? RefDate, string? JobNo)> entries,
            DateTime? invoiceDate)
        {
            if (entries == null || entries.Count == 0)
                return null;

            if (!invoiceDate.HasValue)
                return entries.OrderByDescending(e => e.RefDate).FirstOrDefault().JobNo;

            var invDateOnly = DateOnly.FromDateTime(invoiceDate.Value);

            var onOrBefore = entries
                .Where(e => e.RefDate.HasValue && e.RefDate.Value <= invDateOnly)
                .OrderByDescending(e => e.RefDate.Value)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(onOrBefore.JobNo))
                return onOrBefore.JobNo;

            return entries
                .OrderBy(e => e.RefDate.HasValue
                    ? Math.Abs((e.RefDate.Value.DayNumber - invDateOnly.DayNumber))
                    : int.MaxValue)
                .FirstOrDefault().JobNo;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? dealerCode,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                dealerCode = (dealerCode ?? "").Trim();
                if (dealerCode.Equals("undefined", StringComparison.OrdinalIgnoreCase) ||
                    dealerCode.Equals("null", StringComparison.OrdinalIgnoreCase))
                    dealerCode = "";

                if (!IsAdmin() && string.IsNullOrEmpty(dealerCode))
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Dealer code is required." });

                pageIndex = Math.Max(1, pageIndex);
                pageSize = Math.Clamp(pageSize, 10, 500);

                DateTime? from = fromDate?.Date;
                DateTime? toExclusive = toDate?.Date.AddDays(1);

                var query = _context.DmsRepairBills.AsNoTracking();

                if (from.HasValue)
                    query = query.Where(h => h.InvoiceDate >= from.Value);
                if (toExclusive.HasValue)
                    query = query.Where(h => h.InvoiceDate < toExclusive.Value);
                if (!string.IsNullOrEmpty(dealerCode))
                    query = query.Where(h => h.DealerCode == dealerCode);

                var totalRecords = await query.CountAsync();

                var rawRows = await query
                    .OrderByDescending(h => h.InvoiceDate)
                    .ThenByDescending(h => h.Id)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(h => new
                    {
                        id = h.Id,
                        dealerCode = h.DealerCode,
                        dealerName = h.DealerName,
                        invoiceNo = h.InvoiceNo,
                        invoiceDate = h.InvoiceDate,
                        location = h.Location,
                        partyName = h.PartyName,
                        billType = h.BillType.ToString(),
                        cashType = h.CashType,
                        regNo = h.RegNo,
                        chassisNo = h.ChassisNo
                    })
                    .ToListAsync();

                var chassisList = rawRows
                    .Select(r => r.chassisNo)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();

                var lineOrderQuery = _context.DmsLineOrderReports
                    .AsNoTracking()
                    .Where(l => l.JobNo != null && l.JobNo != "" && l.ChassisNo != null && l.ChassisNo != "");

                if (!string.IsNullOrEmpty(dealerCode))
                    lineOrderQuery = lineOrderQuery.Where(l => l.DealerCode == dealerCode);

                if (chassisList.Count > 0)
                    lineOrderQuery = lineOrderQuery.Where(l => chassisList.Contains(l.ChassisNo!));

                var lineOrders = await lineOrderQuery
                    .Select(l => new
                    {
                        l.DealerCode,
                        l.ChassisNo,
                        RefDate = l.JobDate ?? l.DocDate,
                        l.JobNo
                    })
                    .ToListAsync();

                var jobLookup = lineOrders
                    .GroupBy(l => $"{(l.DealerCode ?? "").Trim().ToUpper()}|{(l.ChassisNo ?? "").Trim().ToUpper()}")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => (x.RefDate, x.JobNo)).ToList()
                    );

                var rows = rawRows.Select(r =>
                {
                    var key = $"{(r.dealerCode ?? "").Trim().ToUpper()}|{(r.chassisNo ?? "").Trim().ToUpper()}";
                    jobLookup.TryGetValue(key, out var listForChassis);
                    return new
                    {
                        r.id,
                        r.dealerCode,
                        r.dealerName,
                        r.invoiceNo,
                        r.invoiceDate,
                        r.location,
                        r.partyName,
                        r.billType,
                        r.cashType,
                        r.regNo,
                        r.chassisNo,
                        jobNo = ResolveJobNo(listForChassis!, r.invoiceDate)
                    };
                }).ToList();

                return Ok(new
                {
                    totalRecords,
                    pageIndex,
                    pageSize,
                    data = rows
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading historical repair bills");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("items-by-bills")]
        public async Task<IActionResult> GetItemsByBills([FromBody] int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                    return Ok(new List<DmsRepairBillItem>());

                var items = await _context.DmsRepairBillItems
                    .AsNoTracking()
                    .Where(i => ids.Contains(i.RepairBillId))
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading historical repair bill items");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? dealerCode)
        {
            try
            {
                dealerCode = (dealerCode ?? "").Trim();
                if (dealerCode.Equals("undefined", StringComparison.OrdinalIgnoreCase) ||
                    dealerCode.Equals("null", StringComparison.OrdinalIgnoreCase))
                    dealerCode = "";

                if (!IsAdmin() && string.IsNullOrEmpty(dealerCode))
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Dealer code is required." });

                DateTime? from = fromDate?.Date;
                DateTime? toExclusive = toDate?.Date.AddDays(1);

                var billQuery = _context.DmsRepairBills.AsNoTracking();

                if (from.HasValue)
                    billQuery = billQuery.Where(h => h.InvoiceDate >= from.Value);
                if (toExclusive.HasValue)
                    billQuery = billQuery.Where(h => h.InvoiceDate < toExclusive.Value);
                if (!string.IsNullOrEmpty(dealerCode))
                    billQuery = billQuery.Where(h => h.DealerCode == dealerCode);

                var rawExportData = await (
                    from b in billQuery
                    join i in _context.DmsRepairBillItems.AsNoTracking()
                        on b.Id equals i.RepairBillId into itemGroup
                    from item in itemGroup.DefaultIfEmpty()
                    orderby b.InvoiceDate descending, b.Id descending, item.Id ascending
                    select new
                    {
                        dealerCode = b.DealerCode,
                        dealerName = b.DealerName,
                        invoiceNo = b.InvoiceNo,
                        invoiceDate = b.InvoiceDate,
                        location = b.Location,
                        partyName = b.PartyName,
                        billType = b.BillType,
                        cashType = b.CashType,
                        regNo = b.RegNo,
                        chassisNo = b.ChassisNo,
                        itemCode = item != null ? item.ItemCode : null,
                        itemDesc = item != null ? item.ItemDesc : null,
                        itemType = item != null ? item.ItemType : null,
                        issueType = item != null ? item.IssueType : null,
                        qty = item != null ? item.Qty : null,
                        rate = item != null ? item.Rate : null,
                        sgstPer = item != null ? item.SgstPer : null,
                        sgstAmount = item != null ? item.SgstAmount : null,
                        cgstPer = item != null ? item.CgstPer : null,
                        cgstAmount = item != null ? item.CgstAmount : null,
                        igstPer = item != null ? item.IgstPer : null,
                        igstAmount = item != null ? item.IgstAmount : null,
                        totAmnt = item != null ? item.TotAmnt : null
                    }
                ).ToListAsync();

                var chassisList = rawExportData
                    .Select(r => r.chassisNo)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();

                var lineOrderQuery = _context.DmsLineOrderReports
                    .AsNoTracking()
                    .Where(l => l.JobNo != null && l.JobNo != "" && l.ChassisNo != null && l.ChassisNo != "");

                if (!string.IsNullOrEmpty(dealerCode))
                    lineOrderQuery = lineOrderQuery.Where(l => l.DealerCode == dealerCode);

                if (chassisList.Count > 0)
                    lineOrderQuery = lineOrderQuery.Where(l => chassisList.Contains(l.ChassisNo!));

                var lineOrders = await lineOrderQuery
                    .Select(l => new
                    {
                        l.DealerCode,
                        l.ChassisNo,
                        RefDate = l.JobDate ?? l.DocDate,
                        l.JobNo
                    })
                    .ToListAsync();

                var jobLookup = lineOrders
                    .GroupBy(l => $"{(l.DealerCode ?? "").Trim().ToUpper()}|{(l.ChassisNo ?? "").Trim().ToUpper()}")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => (x.RefDate, x.JobNo)).ToList()
                    );

                var exportData = rawExportData.Select(r =>
                {
                    var key = $"{(r.dealerCode ?? "").Trim().ToUpper()}|{(r.chassisNo ?? "").Trim().ToUpper()}";
                    jobLookup.TryGetValue(key, out var listForChassis);
                    return new
                    {
                        r.dealerCode,
                        r.dealerName,
                        r.invoiceNo,
                        r.invoiceDate,
                        r.location,
                        r.partyName,
                        r.billType,
                        r.cashType,
                        r.regNo,
                        r.chassisNo,
                        jobNo = ResolveJobNo(listForChassis!, r.invoiceDate),
                        r.itemCode,
                        r.itemDesc,
                        r.itemType,
                        r.issueType,
                        r.qty,
                        r.rate,
                        r.sgstPer,
                        r.sgstAmount,
                        r.cgstPer,
                        r.cgstAmount,
                        r.igstPer,
                        r.igstAmount,
                        r.totAmnt
                    };
                }).ToList();

                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting historical repair bills");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}