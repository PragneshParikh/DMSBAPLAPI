using System.Security.Claims;
using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/historical-material-transfer")]
    [ApiController]
    public class HistoricalMaterialTransferController : ControllerBase
    {
        private readonly BapldmsvadContext _context;
        private readonly ILogger<HistoricalMaterialTransferController> _logger;

        public HistoricalMaterialTransferController(
            BapldmsvadContext context,
            ILogger<HistoricalMaterialTransferController> logger)
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

        private static string? ResolveTransferJobNo(
            Dictionary<string, string?> itemDateJobMap,
            Dictionary<string, string?> dateJobMap,
            string? dealerCode,
            DateTime? docDate,
            List<string?> itemNames)
        {
            var dealer = (dealerCode ?? "").Trim().ToUpper();
            if (!docDate.HasValue)
                return null;

            var dateStr = DateOnly.FromDateTime(docDate.Value).ToString("yyyy-MM-dd");

            foreach (var rawItem in itemNames)
            {
                var item = (rawItem ?? "").Trim().ToUpper();
                if (string.IsNullOrEmpty(item))
                    continue;

                var itemKey = $"{dealer}|{dateStr}|{item}";
                if (itemDateJobMap.TryGetValue(itemKey, out var matchedJob) && !string.IsNullOrWhiteSpace(matchedJob))
                    return matchedJob;
            }

            var dateKey = $"{dealer}|{dateStr}";
            return dateJobMap.GetValueOrDefault(dateKey);
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

                var query = _context.DmsMaterialTransfers.AsNoTracking();

                if (from.HasValue)
                    query = query.Where(h => h.DocDate >= from.Value);
                if (toExclusive.HasValue)
                    query = query.Where(h => h.DocDate < toExclusive.Value);
                if (!string.IsNullOrEmpty(dealerCode))
                    query = query.Where(h => h.DealerCode == dealerCode);

                var totalRecords = await query.CountAsync();

                var rawRows = await query
                    .OrderBy(h => h.DocDate)
                    .ThenBy(h => h.Id)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(h => new
                    {
                        id = h.Id,
                        dealerCode = h.DealerCode,
                        dealerName = h.DealerName,
                        docNo = h.DocNo,
                        docDate = h.DocDate,
                        docType = h.DocType,
                        location = h.Location,
                        locCode = h.LocCode,
                        technicianName = h.TechnicianName,
                        action = h.Action
                    })
                    .ToListAsync();

                var transferIds = rawRows.Select(r => r.id).ToList();

                var transferItems = transferIds.Count > 0
                    ? await _context.DmsMaterialTransferItems
                        .AsNoTracking()
                        .Where(i => transferIds.Contains(i.MaterialTransferId))
                        .Select(i => new AnonymousTransferItem
                        {
                            MaterialTransferId = i.MaterialTransferId,
                            ItemName = i.ItemName
                        })
                        .ToListAsync()
                    : new List<AnonymousTransferItem>();

                var itemsByTransfer = transferItems
                    .GroupBy(i => i.MaterialTransferId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ItemName).ToList());

                var docDates = rawRows
                    .Where(r => r.docDate.HasValue)
                    .Select(r => DateOnly.FromDateTime(r.docDate!.Value))
                    .Distinct()
                    .ToList();

                var lineOrderQuery = _context.DmsLineOrderReports
                    .AsNoTracking()
                    .Where(l => l.JobNo != null && l.JobNo != "" && l.DocDate.HasValue);

                if (!string.IsNullOrEmpty(dealerCode))
                    lineOrderQuery = lineOrderQuery.Where(l => l.DealerCode == dealerCode);

                if (docDates.Count > 0)
                    lineOrderQuery = lineOrderQuery.Where(l => docDates.Contains(l.DocDate!.Value));

                var lineOrders = await lineOrderQuery
                    .Select(l => new
                    {
                        l.DealerCode,
                        l.DocDate,
                        l.ItemName,
                        l.JobNo
                    })
                    .ToListAsync();

                var itemDateJobMap = lineOrders
                    .Where(l => !string.IsNullOrWhiteSpace(l.ItemName))
                    .GroupBy(l => $"{(l.DealerCode ?? "").Trim().ToUpper()}|{l.DocDate!.Value:yyyy-MM-dd}|{(l.ItemName ?? "").Trim().ToUpper()}")
                    .ToDictionary(g => g.Key, g => g.First().JobNo);

                var dateJobMap = lineOrders
                    .GroupBy(l => $"{(l.DealerCode ?? "").Trim().ToUpper()}|{l.DocDate!.Value:yyyy-MM-dd}")
                    .ToDictionary(g => g.Key, g => g.First().JobNo);

                var rows = rawRows.Select(r =>
                {
                    itemsByTransfer.TryGetValue(r.id, out var itemNames);
                    return new
                    {
                        r.id,
                        r.dealerCode,
                        r.dealerName,
                        r.docNo,
                        r.docDate,
                        r.docType,
                        r.location,
                        r.locCode,
                        r.technicianName,
                        r.action,
                        jobNo = ResolveTransferJobNo(itemDateJobMap, dateJobMap, r.dealerCode, r.docDate, itemNames ?? new List<string?>())
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
                _logger.LogError(ex, "Error occurred while loading historical material transfers");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private sealed class AnonymousTransferItem
        {
            public int MaterialTransferId { get; set; }
            public string? ItemName { get; set; }
        }

        [HttpPost("details-by-transfers")]
        public async Task<IActionResult> GetDetailsByTransfers([FromBody] int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                    return Ok(new { items = new List<DmsMaterialTransferItem>(), labors = new List<DmsMaterialTransferLabor>() });

                var items = await _context.DmsMaterialTransferItems
                    .AsNoTracking()
                    .Where(i => ids.Contains(i.MaterialTransferId))
                    .ToListAsync();

                var itemIds = items.Select(i => i.Id).Distinct().ToList();

                var labors = itemIds.Count > 0
                    ? await _context.DmsMaterialTransferLabors
                        .AsNoTracking()
                        .Where(l => itemIds.Contains(l.MaterialTransferItemId))
                        .ToListAsync()
                    : new List<DmsMaterialTransferLabor>();

                return Ok(new { items, labors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading historical material transfer details");
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

                var transferQuery = _context.DmsMaterialTransfers.AsNoTracking();

                if (from.HasValue)
                    transferQuery = transferQuery.Where(h => h.DocDate >= from.Value);
                if (toExclusive.HasValue)
                    transferQuery = transferQuery.Where(h => h.DocDate < toExclusive.Value);
                if (!string.IsNullOrEmpty(dealerCode))
                    transferQuery = transferQuery.Where(h => h.DealerCode == dealerCode);

                var rawExportData = await (
                    from t in transferQuery
                    join i in _context.DmsMaterialTransferItems.AsNoTracking()
                        on t.Id equals i.MaterialTransferId into itemGroup
                    from item in itemGroup.DefaultIfEmpty()
                    join l in _context.DmsMaterialTransferLabors.AsNoTracking()
                        on (item != null ? item.Id : -1) equals l.MaterialTransferItemId into laborGroup
                    from labor in laborGroup.DefaultIfEmpty()
                    orderby t.DocDate, t.Id, item.Id, labor.Id
                    select new
                    {
                        dealerCode = t.DealerCode,
                        dealerName = t.DealerName,
                        docNo = t.DocNo,
                        docDate = t.DocDate,
                        docType = t.DocType,
                        location = t.Location,
                        locCode = t.LocCode,
                        technicianName = t.TechnicianName,
                        action = t.Action,
                        itemName = item != null ? item.ItemName : null,
                        itemDescription = item != null ? item.ItemDescription : null,
                        itemType = item != null ? item.ItemType : null,
                        qty = item != null ? (decimal?)item.Qty : null,
                        rate = item != null ? (decimal?)item.Rate : null,
                        discount = item != null ? (decimal?)item.Discount : null,
                        mrp = item != null ? (decimal?)item.Mrp : null,
                        itemSgstAmount = item != null ? (decimal?)item.SgstAmount : null,
                        itemCgstAmount = item != null ? (decimal?)item.CgstAmount : null,
                        itemIgstAmount = item != null ? (decimal?)item.IgstAmount : null,
                        lbrName = labor != null ? labor.LbrName : null,
                        lbrDescription = labor != null ? labor.LbrDescription : null,
                        lbrRate = labor != null ? (decimal?)labor.LbrRate : null,
                        lbrSgstAmount = labor != null ? (decimal?)labor.SgstAmount : null,
                        lbrCgstAmount = labor != null ? (decimal?)labor.CgstAmount : null,
                        lbrIgstAmount = labor != null ? (decimal?)labor.IgstAmount : null
                    }
                ).ToListAsync();

                var docDates = rawExportData
                    .Where(r => r.docDate.HasValue)
                    .Select(r => DateOnly.FromDateTime(r.docDate!.Value))
                    .Distinct()
                    .ToList();

                var lineOrderQuery = _context.DmsLineOrderReports
                    .AsNoTracking()
                    .Where(l => l.JobNo != null && l.JobNo != "" && l.DocDate.HasValue);

                if (!string.IsNullOrEmpty(dealerCode))
                    lineOrderQuery = lineOrderQuery.Where(l => l.DealerCode == dealerCode);

                if (docDates.Count > 0)
                    lineOrderQuery = lineOrderQuery.Where(l => docDates.Contains(l.DocDate!.Value));

                var lineOrders = await lineOrderQuery
                    .Select(l => new
                    {
                        l.DealerCode,
                        l.DocDate,
                        l.ItemName,
                        l.JobNo
                    })
                    .ToListAsync();

                var itemDateJobMap = lineOrders
                    .Where(l => !string.IsNullOrWhiteSpace(l.ItemName))
                    .GroupBy(l => $"{(l.DealerCode ?? "").Trim().ToUpper()}|{l.DocDate!.Value:yyyy-MM-dd}|{(l.ItemName ?? "").Trim().ToUpper()}")
                    .ToDictionary(g => g.Key, g => g.First().JobNo);

                var dateJobMap = lineOrders
                    .GroupBy(l => $"{(l.DealerCode ?? "").Trim().ToUpper()}|{l.DocDate!.Value:yyyy-MM-dd}")
                    .ToDictionary(g => g.Key, g => g.First().JobNo);

                var exportData = rawExportData.Select(r => new
                {
                    r.dealerCode,
                    r.dealerName,
                    r.docNo,
                    r.docDate,
                    jobNo = ResolveTransferJobNo(itemDateJobMap, dateJobMap, r.dealerCode, r.docDate, new List<string?> { r.itemName }),
                    r.docType,
                    r.location,
                    r.locCode,
                    r.technicianName,
                    r.action,
                    r.itemName,
                    r.itemDescription,
                    r.itemType,
                    r.qty,
                    r.rate,
                    r.discount,
                    r.mrp,
                    r.itemSgstAmount,
                    r.itemCgstAmount,
                    r.itemIgstAmount,
                    r.lbrName,
                    r.lbrDescription,
                    r.lbrRate,
                    r.lbrSgstAmount,
                    r.lbrCgstAmount,
                    r.lbrIgstAmount
                }).ToList();

                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting historical material transfers");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}