using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMSBAPLAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/new-zero-hours")]
    public class NewZeroHoursController : ControllerBase
    {
        private readonly BapldmsvadContext _context;

        public NewZeroHoursController(BapldmsvadContext context)
        {
            _context = context;
        }

        public class SyncZeroHoursRequest
        {
            public string? DealerCode { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
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

        private IQueryable<NewZeroHoursJobHeader> BuildFilteredQuery(
            string? dealerCode,
            string? locationCode,
            DateTime? fromDate,
            DateTime? toDate,
            string? search)
        {
            var query = _context.NewZeroHoursJobHeaders
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (!IsSuperAdmin())
            {
                var userDealer = GetUserDealerCode();
                if (string.IsNullOrWhiteSpace(userDealer))
                    throw new UnauthorizedAccessException("DealerCode claim missing.");

                // 26.11.2026 - a BG employee has several dealers: the chosen one (if it is theirs) or all of them
                var scoped = GetUserInfoFromToken.SplitCodes(GetUserInfoFromToken.ScopeDealerCode(HttpContext, dealerCode));
                query = query.Where(x => scoped.Contains(x.DealerCode));
            }
            else if (!string.IsNullOrWhiteSpace(dealerCode) &&
                     !string.Equals(dealerCode.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var dc = dealerCode.Trim();
                query = query.Where(x => x.DealerCode == dc);
            }

            if (!string.IsNullOrWhiteSpace(locationCode) &&
                !string.Equals(locationCode.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var loc = locationCode.Trim();
                query = query.Where(x => x.LocationCode == loc);
            }

            if (fromDate.HasValue)
            {
                var from = DateOnly.FromDateTime(fromDate.Value.Date);
                query = query.Where(x => x.JobDate >= from);
            }

            if (toDate.HasValue)
            {
                var to = DateOnly.FromDateTime(toDate.Value.Date);
                query = query.Where(x => x.JobDate <= to);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    (x.JobNo != null && x.JobNo.Contains(s)) ||
                    (x.DocNo != null && x.DocNo.Contains(s)) ||
                    (x.ChassisNo != null && x.ChassisNo.Contains(s)) ||
                    (x.MotorNo != null && x.MotorNo.Contains(s)) ||
                    (x.PartyName != null && x.PartyName.Contains(s)) ||
                    (x.PartyMobile != null && x.PartyMobile.Contains(s)) ||
                    (x.RegNo != null && x.RegNo.Contains(s)));
            }

            return query;
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations([FromQuery] string? dealerCode)
        {
            try
            {
                string? targetDealer = null;

                if (!IsSuperAdmin())
                {
                    if (string.IsNullOrWhiteSpace(GetUserDealerCode()))
                        return Unauthorized(new { message = "DealerCode claim missing." });
                    // 26.11.2026 - chosen dealer if it is one of theirs, otherwise all of theirs
                    targetDealer = GetUserInfoFromToken.ScopeDealerCode(HttpContext, dealerCode);
                }
                else if (!string.IsNullOrWhiteSpace(dealerCode) &&
                         !string.Equals(dealerCode.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    targetDealer = dealerCode.Trim();
                }

                var locQuery = _context.LocationMasters
                    .AsNoTracking()
                    .Where(l => l.Active == "Y" && !string.IsNullOrEmpty(l.Loccode));

                if (!string.IsNullOrWhiteSpace(targetDealer))
                {
                    var targets = GetUserInfoFromToken.SplitCodes(targetDealer);
                    locQuery = locQuery.Where(l => targets.Contains(l.Dealercode));
                }

                var locations = await locQuery
                    .Select(l => new
                    {
                        locationCode = l.Loccode!.Trim(),
                        locationName = (l.Locname ?? l.Loccode)!.Trim()
                    })
                    .Distinct()
                    .OrderBy(l => l.locationCode)
                    .ToListAsync();

                return Ok(new { data = locations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> List(
            [FromQuery] string? DealerCode,
            [FromQuery] string? LocationCode,
            [FromQuery] DateTime? FromDate,
            [FromQuery] DateTime? ToDate,
            [FromQuery] string? Search,
            [FromQuery] int PageIndex = 1,
            [FromQuery] int PageSize = 25)
        {
            try
            {
                var query = BuildFilteredQuery(DealerCode, LocationCode, FromDate, ToDate, Search);

                var totalRecords = await query.CountAsync();

                PageIndex = Math.Max(1, PageIndex);
                PageSize = Math.Clamp(PageSize, 1, 500);

                var data = await query
                    .OrderByDescending(x => x.JobDate)
                    .ThenByDescending(x => x.Id)
                    .Skip((PageIndex - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                return Ok(new { data, totalRecords });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }


        //[HttpPost("sync")]
        //public async Task<IActionResult> Sync([FromBody] SyncZeroHoursRequest? request)
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        string? targetDealer = null;

        //        if (!IsSuperAdmin())
        //        {
        //            targetDealer = GetUserDealerCode();
        //            if (string.IsNullOrWhiteSpace(targetDealer))
        //                return Unauthorized(new { message = "DealerCode claim missing." });
        //        }
        //        else if (!string.IsNullOrWhiteSpace(request?.DealerCode) &&
        //                 !string.Equals(request.DealerCode.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
        //        {
        //            targetDealer = request.DealerCode.Trim();
        //        }

        //        var jcQuery = _context.JobCardHeaders
        //            .AsNoTracking()
        //            .Where(j => j.IsDelete != true);

        //        if (!string.IsNullOrWhiteSpace(targetDealer))
        //        {
        //            jcQuery = jcQuery.Where(j => j.DealerCode == targetDealer);
        //        }

        //        if (request?.FromDate.HasValue == true)
        //        {
        //            var from = DateOnly.FromDateTime(request.FromDate.Value.Date);
        //            jcQuery = jcQuery.Where(j => j.JobinDate >= from);
        //        }

        //        if (request?.ToDate.HasValue == true)
        //        {
        //            var to = DateOnly.FromDateTime(request.ToDate.Value.Date);
        //            jcQuery = jcQuery.Where(j => j.JobinDate <= to);
        //        }

        //        var jobCards = await jcQuery
        //            .OrderByDescending(j => j.JobinDate)
        //            .ThenByDescending(j => j.Id)
        //            .ToListAsync();

        //        if (jobCards.Count == 0)
        //        {
        //            await transaction.CommitAsync();
        //            return Ok(new { message = "Synced 0 new record(s).", syncedCount = 0 });
        //        }

        //        var jobCardIds = jobCards.Select(j => j.Id).ToList();

        //        var customersByJobId = await _context.JobCardCustomers
        //            .AsNoTracking()
        //            .Where(c => jobCardIds.Contains(c.JobCardHeaderId))
        //            .OrderByDescending(c => c.Id)
        //            .GroupBy(c => c.JobCardHeaderId)
        //            .ToDictionaryAsync(g => g.Key, g => g.First());

        //        var locations = await _context.LocationMasters
        //            .AsNoTracking()
        //            .Where(l => !string.IsNullOrWhiteSpace(l.Loccode))
        //            .Select(l => new { l.Dealercode, l.Loccode, l.Locname })
        //            .ToListAsync();

        //        var locByDealerAndCode = locations
        //            .Where(l => !string.IsNullOrWhiteSpace(l.Dealercode))
        //            .GroupBy(l => $"{l.Dealercode!.Trim()}|{l.Loccode!.Trim()}", StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        //        var locByCodeOnly = locations
        //            .GroupBy(l => l.Loccode!.Trim(), StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        //        var existingHeaders = await _context.NewZeroHoursJobHeaders
        //            .Include(h => h.NewZeroHoursJobDetails)
        //            .Where(h => !h.IsDelete && h.JobCardHeaderId.HasValue && jobCardIds.Contains(h.JobCardHeaderId.Value))
        //            .ToListAsync();

        //        var existingByKey = existingHeaders
        //            .GroupBy(h => $"{(h.DealerCode ?? "").Trim()}|{h.JobCardHeaderId}", StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        //        var materialsByJobId = await _context.MaterialTransfers
        //            .AsNoTracking()
        //            .Where(m => !m.IsDelete && jobCardIds.Contains(m.JobId))
        //            .GroupBy(m => m.JobId)
        //            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        //        var repairBillsByJobId = await _context.RepairBillHeaders
        //            .AsNoTracking()
        //            .Include(r => r.RepairBillDetails)
        //            .Where(r => !r.IsDelete && jobCardIds.Contains(r.JobId))
        //            .GroupBy(r => r.JobId)
        //            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        //        var userName = GetCurrentUserName();
        //        var now = DateTime.Now;
        //        int syncedCount = 0;

        //        foreach (var jc in jobCards)
        //        {
        //            var dc = (jc.DealerCode ?? targetDealer ?? "").Trim();
        //            if (string.IsNullOrWhiteSpace(dc))
        //                continue;

        //            var locCode = jc.Serviceloc?.Trim();
        //            string? locName = null;
        //            if (!string.IsNullOrWhiteSpace(locCode))
        //            {
        //                if (locByDealerAndCode.TryGetValue($"{dc}|{locCode}", out var locMatch))
        //                    locName = locMatch.Locname?.Trim();
        //                else if (locByCodeOnly.TryGetValue(locCode, out var locFallback))
        //                    locName = locFallback.Locname?.Trim();
        //            }

        //            customersByJobId.TryGetValue(jc.Id, out var cust);

        //            var key = $"{dc}|{jc.Id}";
        //            existingByKey.TryGetValue(key, out var header);

        //            if (header == null)
        //            {
        //                header = new NewZeroHoursJobHeader
        //                {
        //                    CreatedBy = userName,
        //                    CreatedDate = now,
        //                    IsActive = true,
        //                    IsDelete = false
        //                };
        //                _context.NewZeroHoursJobHeaders.Add(header);
        //                existingByKey[key] = header;
        //                syncedCount++;
        //            }
        //            else
        //            {
        //                header.UpdatedBy = userName;
        //                header.UpdatedDate = now;
        //            }

        //            var jobBills = repairBillsByJobId.TryGetValue(jc.Id, out var bills)
        //                ? bills
        //                : new List<RepairBillHeader>();

        //            header.JobCardHeaderId = jc.Id;
        //            header.RepairBillHeaderId = jobBills.OrderByDescending(b => b.Id).FirstOrDefault()?.Id;
        //            header.DealerCode = dc;
        //            header.LocationCode = locCode;
        //            header.LocationName = locName;
        //            header.JobNo = jc.JobNo?.ToString();
        //            header.JobDate = jc.JobinDate;
        //            header.JobType = jc.Jobtype?.ToString();
        //            header.KMS = jc.Vehiclekms;
        //            header.ServiceHead = jc.Servicehead?.ToString();
        //            header.CopnNo = jc.Couponno;
        //            header.Supervisor = jc.Supervisor;
        //            header.Technician = jc.Technician;
        //            header.ExpectedDeliveryDate = jc.EstdelDate;
        //            header.JobStatus = jc.JobStatus;
        //            header.CreateThrough = "DMS_SYNC";

        //            header.PartyName = cust?.CustomerName;
        //            header.PartyMobile = cust?.CustomerMobile;
        //            header.Model = cust?.ModelName;
        //            header.ChassisNo = cust?.ChassisNo ?? jc.Chassisno;
        //            header.MotorNo = cust?.MotorNo;
        //            header.RegNo = cust?.RegisterNo;
        //            header.SaleDate = cust?.SaleDate.HasValue == true
        //                ? DateOnly.FromDateTime(cust.SaleDate.Value)
        //                : null;

        //            foreach (var oldDetail in header.NewZeroHoursJobDetails.Where(d => !d.IsDelete))
        //            {
        //                oldDetail.IsDelete = true;
        //            }

        //            decimal totalParts = 0m;
        //            decimal totalLabour = 0m;
        //            decimal totalTaxable = 0m;
        //            decimal totalGst = 0m;
        //            decimal totalIgst = 0m;
        //            decimal totalNet = 0m;

        //            // 1. MaterialTransfer -> Details
        //            if (materialsByJobId.TryGetValue(jc.Id, out var matList))
        //            {
        //                foreach (var mt in matList)
        //                {
        //                    decimal qty = mt.Quantity;
        //                    decimal rate = mt.ItemRate;
        //                    decimal taxable = qty * rate;
        //                    decimal net = taxable;

        //                    totalParts += net;
        //                    totalTaxable += taxable;
        //                    totalNet += net;

        //                    header.NewZeroHoursJobDetails.Add(new NewZeroHoursJobDetail
        //                    {
        //                        JobCardHeaderId = jc.Id,
        //                        MaterialTransferId = mt.Id,
        //                        SourceType = "Material",
        //                        ItemCode = mt.ItemCode,
        //                        ItemDesc = mt.ItemDesc,
        //                        IssueType = mt.IssueType.ToString(),
        //                        Qty = qty,
        //                        Rate = rate,
        //                        TaxableAmount = taxable,
        //                        GstAmount = 0m,
        //                        IgstAmount = 0m,
        //                        NetAmount = net,
        //                        IsDelete = false,
        //                        CreatedDate = now
        //                    });
        //                }
        //            }

        //            // 2. RepairBillHeader + RepairBillDetail -> Details
        //            foreach (var rb in jobBills)
        //            {
        //                var billDetails = rb.RepairBillDetails?.ToList() ?? new List<RepairBillDetail>();
        //                foreach (var rbd in billDetails)
        //                {
        //                    var itemType = (rbd.ItemType ?? string.Empty).Trim();
        //                    bool isLabour = itemType.Contains("Labour", StringComparison.OrdinalIgnoreCase) ||
        //                                    itemType.Contains("Labor", StringComparison.OrdinalIgnoreCase) ||
        //                                    rbd.LabourMasterId.HasValue ||
        //                                    rbd.PartWiseLabourId.HasValue;

        //                    decimal qty = isLabour
        //                        ? (rbd.LabourQty ?? 0m)
        //                        : (rbd.PartQty ?? 0m);

        //                    decimal rate = isLabour
        //                        ? (rbd.LabourRate ?? 0m)
        //                        : (rbd.PartRate ?? 0m);

        //                    decimal taxable = isLabour
        //                        ? (rbd.LabourTaxblAmount ?? (qty * rate))
        //                        : (rbd.PartTaxblAmount ?? (qty * rate));

        //                    decimal gst = (rbd.Cgstamount ?? 0m) + (rbd.Sgstamount ?? 0m);
        //                    decimal igst = rbd.Igstamount ?? 0m;

        //                    decimal net = isLabour
        //                        ? (rbd.LabourNetAmount ?? (taxable + gst + igst))
        //                        : (rbd.PartNetAmount ?? (taxable + gst + igst));

        //                    if (isLabour)
        //                        totalLabour += net;
        //                    else
        //                        totalParts += net;

        //                    totalTaxable += taxable;
        //                    totalGst += gst;
        //                    totalIgst += igst;
        //                    totalNet += net;

        //                    header.NewZeroHoursJobDetails.Add(new NewZeroHoursJobDetail
        //                    {
        //                        JobCardHeaderId = jc.Id,
        //                        RepairBillId = rb.Id,
        //                        MaterialTransferId = rbd.MaterialId,
        //                        SourceType = isLabour ? "Labour" : (string.IsNullOrWhiteSpace(itemType) ? "Part" : itemType),
        //                        ItemCode = rbd.PartItem?.Itemcode ?? rbd.PartItemId?.ToString() ?? rbd.LabourMasterId?.ToString(),
        //                        ItemDesc = rbd.PartItem?.Itemdesc ?? itemType,
        //                        IssueType = rbd.IssutypeId?.ToString(),
        //                        Qty = qty,
        //                        Rate = rate,
        //                        TaxableAmount = taxable,
        //                        GstAmount = gst,
        //                        IgstAmount = igst,
        //                        NetAmount = net,
        //                        IsDelete = false,
        //                        CreatedDate = now
        //                    });
        //                }
        //            }

        //            header.PartsAmount = totalParts;
        //            header.LabourAmount = totalLabour;
        //            header.TotalWoTax = totalTaxable;
        //            header.GstAmnt = totalGst;
        //            header.IgstAmnt = totalIgst;
        //            header.NetTotal = totalNet;
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Ok(new
        //        {
        //            message = $"Synced {syncedCount} new record(s) successfully.",
        //            syncedCount
        //        });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = ex.GetBaseException().Message });
        //    }
        //}

        //[HttpPost("sync")]
        //public async Task<IActionResult> Sync([FromBody] SyncZeroHoursRequest? request)
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        string? targetDealer = null;

        //        if (!IsSuperAdmin())
        //        {
        //            targetDealer = GetUserDealerCode();
        //            if (string.IsNullOrWhiteSpace(targetDealer))
        //                return Unauthorized(new { message = "DealerCode claim missing." });
        //        }
        //        else if (!string.IsNullOrWhiteSpace(request?.DealerCode) &&
        //                 !string.Equals(request.DealerCode.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
        //        {
        //            targetDealer = request.DealerCode.Trim();
        //        }

        //        var jcQuery = _context.JobCardHeaders
        //            .AsNoTracking()
        //            .Where(j => j.IsDelete != true);

        //        if (!string.IsNullOrWhiteSpace(targetDealer))
        //        {
        //            jcQuery = jcQuery.Where(j => j.DealerCode == targetDealer);
        //        }

        //        if (request?.FromDate.HasValue == true)
        //        {
        //            var from = DateOnly.FromDateTime(request.FromDate.Value.Date);
        //            jcQuery = jcQuery.Where(j => j.JobinDate >= from);
        //        }

        //        if (request?.ToDate.HasValue == true)
        //        {
        //            var to = DateOnly.FromDateTime(request.ToDate.Value.Date);
        //            jcQuery = jcQuery.Where(j => j.JobinDate <= to);
        //        }

        //        var jobCards = await jcQuery
        //            .OrderByDescending(j => j.JobinDate)
        //            .ThenByDescending(j => j.Id)
        //            .ToListAsync();

        //        if (jobCards.Count == 0)
        //        {
        //            await transaction.CommitAsync();
        //            return Ok(new { message = "Synced 0 new record(s).", syncedCount = 0 });
        //        }

        //        var jobCardIds = jobCards.Select(j => j.Id).ToList();

        //        var serviceHeadMap = await _context.ServiceHeads
        //            .AsNoTracking()
        //            .Where(s => !string.IsNullOrWhiteSpace(s.ServiceHeadName))
        //            .ToDictionaryAsync(
        //                s => s.Id.ToString(),
        //                s => s.ServiceHeadName!.Trim()
        //            );

        //        var jobTypeMap = await _context.JobTypes
        //            .AsNoTracking()
        //            .Where(jt => !string.IsNullOrWhiteSpace(jt.JobTypeName))
        //            .ToDictionaryAsync(
        //                jt => jt.Id.ToString(),
        //                jt => jt.JobTypeName!.Trim()
        //            );

        //        var customersByJobId = await _context.JobCardCustomers
        //            .AsNoTracking()
        //            .Where(c => jobCardIds.Contains(c.JobCardHeaderId))
        //            .OrderByDescending(c => c.Id)
        //            .GroupBy(c => c.JobCardHeaderId)
        //            .ToDictionaryAsync(g => g.Key, g => g.First());

        //        var locations = await _context.LocationMasters
        //            .AsNoTracking()
        //            .Where(l => !string.IsNullOrWhiteSpace(l.Loccode))
        //            .Select(l => new { l.Dealercode, l.Loccode, l.Locname })
        //            .ToListAsync();

        //        var locByDealerAndCode = locations
        //            .Where(l => !string.IsNullOrWhiteSpace(l.Dealercode))
        //            .GroupBy(l => $"{l.Dealercode!.Trim()}|{l.Loccode!.Trim()}", StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        //        var locByCodeOnly = locations
        //            .GroupBy(l => l.Loccode!.Trim(), StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        //        var existingHeaders = await _context.NewZeroHoursJobHeaders
        //            .Include(h => h.NewZeroHoursJobDetails)
        //            .Where(h => !h.IsDelete && h.JobCardHeaderId.HasValue && jobCardIds.Contains(h.JobCardHeaderId.Value))
        //            .ToListAsync();

        //        var existingByKey = existingHeaders
        //            .GroupBy(h => $"{(h.DealerCode ?? "").Trim()}|{h.JobCardHeaderId}", StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        //        var materialsByJobId = await _context.MaterialTransfers
        //            .AsNoTracking()
        //            .Where(m => !m.IsDelete && jobCardIds.Contains(m.JobId))
        //            .GroupBy(m => m.JobId)
        //            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        //        var repairBillsByJobId = await _context.RepairBillHeaders
        //            .AsNoTracking()
        //            .Include(r => r.RepairBillDetails)
        //                .ThenInclude(d => d.PartItem)
        //            .Where(r => !r.IsDelete && jobCardIds.Contains(r.JobId))
        //            .GroupBy(r => r.JobId)
        //            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        //        var userName = GetCurrentUserName();
        //        var now = DateTime.Now;
        //        int syncedCount = 0;

        //        static string ResolveDealerType(string? partyName)
        //        {
        //            if (string.IsNullOrWhiteSpace(partyName)) return "B2C";
        //            var upper = partyName.ToUpperInvariant();
        //            string[] b2bKeywords = { "PVT", "LTD", "LLP", "MOTORS", "AUTO", "ENTERPRISES", "MOBILITY", "CORP", "INC", "TRADERS", "AGENCIES", "COMPANY", "CO." };
        //            return b2bKeywords.Any(k => upper.Contains(k)) ? "B2B" : "B2C";
        //        }

        //        foreach (var jc in jobCards)
        //        {
        //            var dc = (jc.DealerCode ?? targetDealer ?? "").Trim();
        //            if (string.IsNullOrWhiteSpace(dc))
        //                continue;

        //            var locCode = jc.Serviceloc?.Trim();
        //            string? locName = null;
        //            if (!string.IsNullOrWhiteSpace(locCode))
        //            {
        //                if (locByDealerAndCode.TryGetValue($"{dc}|{locCode}", out var locMatch))
        //                    locName = locMatch.Locname?.Trim();
        //                else if (locByCodeOnly.TryGetValue(locCode, out var locFallback))
        //                    locName = locFallback.Locname?.Trim();
        //            }

        //            customersByJobId.TryGetValue(jc.Id, out var cust);

        //            var key = $"{dc}|{jc.Id}";
        //            existingByKey.TryGetValue(key, out var header);

        //            if (header == null)
        //            {
        //                header = new NewZeroHoursJobHeader
        //                {
        //                    CreatedBy = userName,
        //                    CreatedDate = now,
        //                    IsActive = true,
        //                    IsDelete = false
        //                };
        //                _context.NewZeroHoursJobHeaders.Add(header);
        //                existingByKey[key] = header;
        //                syncedCount++;
        //            }
        //            else
        //            {
        //                header.UpdatedBy = userName;
        //                header.UpdatedDate = now;
        //            }

        //            var jobBills = repairBillsByJobId.TryGetValue(jc.Id, out var bills)
        //                ? bills
        //                : new List<RepairBillHeader>();

        //            var latestBill = jobBills.OrderByDescending(b => b.Id).FirstOrDefault();
        //            var partyName = cust?.CustomerName;
        //            var rawServiceHead = jc.Servicehead?.ToString()?.Trim();
        //            var rawJobType = jc.Jobtype?.ToString()?.Trim();

        //            header.CompName = !string.IsNullOrWhiteSpace(locName) ? locName : null;
        //            header.DealerType = ResolveDealerType(partyName);
        //            header.JobCardHeaderId = jc.Id;
        //            header.RepairBillHeaderId = latestBill?.Id;
        //            header.DealerCode = dc;
        //            header.LocationCode = locCode;
        //            header.LocationName = locName;
        //            header.JobNo = jc.JobNo?.ToString();
        //            header.JobDate = jc.JobinDate;
        //            header.JobType = !string.IsNullOrWhiteSpace(rawJobType) &&
        //                             jobTypeMap.TryGetValue(rawJobType, out var jtName)
        //                ? jtName
        //                : rawJobType;
        //            header.ServiceHead = !string.IsNullOrWhiteSpace(rawServiceHead) &&
        //                                serviceHeadMap.TryGetValue(rawServiceHead, out var shName)
        //                ? shName
        //                : rawServiceHead;
        //            header.DocNo = latestBill != null
        //                ? (!string.IsNullOrWhiteSpace(latestBill.Prefix)
        //                    ? $"{latestBill.Prefix}{latestBill.BillNo}"
        //                    : latestBill.BillNo.ToString())
        //                : (!string.IsNullOrWhiteSpace(jc.InvoiceNo) ? jc.InvoiceNo : jc.EstNo);
        //            header.InvoiceDate = latestBill?.CreatedDate.HasValue == true
        //                ? DateOnly.FromDateTime(latestBill.CreatedDate.Value)
        //                : null;
        //            header.BrandName = string.IsNullOrWhiteSpace(header.BrandName) ? "FGWG" : header.BrandName;
        //            header.KMS = jc.Vehiclekms;
        //            header.CopnNo = jc.Couponno;
        //            header.Supervisor = jc.Supervisor;
        //            header.Technician = jc.Technician;
        //            header.ExpectedDeliveryDate = jc.EstdelDate;
        //            header.JobStatus = jc.JobStatus;
        //            header.CreateThrough = "DMS_SYNC";

        //            header.PartyName = partyName;
        //            header.PartyMobile = cust?.CustomerMobile;
        //            header.Model = cust?.ModelName;
        //            header.ChassisNo = cust?.ChassisNo ?? jc.Chassisno;
        //            header.MotorNo = cust?.MotorNo;
        //            header.RegNo = cust?.RegisterNo;
        //            header.SaleDate = cust?.SaleDate.HasValue == true
        //                ? DateOnly.FromDateTime(cust.SaleDate.Value)
        //                : null;

        //            foreach (var oldDetail in header.NewZeroHoursJobDetails.Where(d => !d.IsDelete))
        //            {
        //                oldDetail.IsDelete = true;
        //            }

        //            decimal totalParts = 0m;
        //            decimal totalLabour = 0m;
        //            decimal totalTaxable = 0m;
        //            decimal totalGst = 0m;
        //            decimal totalIgst = 0m;
        //            decimal totalNet = 0m;

        //            if (materialsByJobId.TryGetValue(jc.Id, out var matList))
        //            {
        //                foreach (var mt in matList)
        //                {
        //                    decimal qty = mt.Quantity;
        //                    decimal rate = mt.ItemRate;
        //                    decimal taxable = qty * rate;
        //                    decimal net = taxable;

        //                    totalParts += net;
        //                    totalTaxable += taxable;
        //                    totalNet += net;

        //                    header.NewZeroHoursJobDetails.Add(new NewZeroHoursJobDetail
        //                    {
        //                        JobCardHeaderId = jc.Id,
        //                        MaterialTransferId = mt.Id,
        //                        SourceType = "Material",
        //                        ItemCode = mt.ItemCode,
        //                        ItemDesc = mt.ItemDesc,
        //                        IssueType = mt.IssueType.ToString(),
        //                        Qty = qty,
        //                        Rate = rate,
        //                        TaxableAmount = taxable,
        //                        GstAmount = 0m,
        //                        IgstAmount = 0m,
        //                        NetAmount = net,
        //                        IsDelete = false,
        //                        CreatedDate = now
        //                    });
        //                }
        //            }

        //            foreach (var rb in jobBills)
        //            {
        //                var billDetails = rb.RepairBillDetails?.ToList() ?? new List<RepairBillDetail>();
        //                foreach (var rbd in billDetails)
        //                {
        //                    var itemType = (rbd.ItemType ?? string.Empty).Trim();
        //                    bool isLabour = itemType.Contains("Labour", StringComparison.OrdinalIgnoreCase) ||
        //                                    itemType.Contains("Labor", StringComparison.OrdinalIgnoreCase) ||
        //                                    rbd.LabourMasterId.HasValue ||
        //                                    rbd.PartWiseLabourId.HasValue;

        //                    decimal qty = isLabour ? (rbd.LabourQty ?? 0m) : (rbd.PartQty ?? 0m);
        //                    decimal rate = isLabour ? (rbd.LabourRate ?? 0m) : (rbd.PartRate ?? 0m);
        //                    decimal taxable = isLabour
        //                        ? (rbd.LabourTaxblAmount ?? (qty * rate))
        //                        : (rbd.PartTaxblAmount ?? (qty * rate));

        //                    decimal gst = (rbd.Cgstamount ?? 0m) + (rbd.Sgstamount ?? 0m);
        //                    decimal igst = rbd.Igstamount ?? 0m;

        //                    decimal net = isLabour
        //                        ? (rbd.LabourNetAmount ?? (taxable + gst + igst))
        //                        : (rbd.PartNetAmount ?? (taxable + gst + igst));

        //                    if (isLabour)
        //                        totalLabour += net;
        //                    else
        //                        totalParts += net;

        //                    totalTaxable += taxable;
        //                    totalGst += gst;
        //                    totalIgst += igst;
        //                    totalNet += net;

        //                    header.NewZeroHoursJobDetails.Add(new NewZeroHoursJobDetail
        //                    {
        //                        JobCardHeaderId = jc.Id,
        //                        RepairBillId = rb.Id,
        //                        MaterialTransferId = rbd.MaterialId,
        //                        SourceType = isLabour ? "Labour" : (string.IsNullOrWhiteSpace(itemType) ? "Part" : itemType),
        //                        ItemCode = rbd.PartItem?.Itemcode ?? rbd.PartItemId?.ToString() ?? rbd.LabourMasterId?.ToString(),
        //                        ItemDesc = rbd.PartItem?.Itemdesc ?? itemType,
        //                        IssueType = rbd.IssutypeId?.ToString(),
        //                        Qty = qty,
        //                        Rate = rate,
        //                        TaxableAmount = taxable,
        //                        GstAmount = gst,
        //                        IgstAmount = igst,
        //                        NetAmount = net,
        //                        IsDelete = false,
        //                        CreatedDate = now
        //                    });
        //                }
        //            }

        //            header.PartsAmount = totalParts;
        //            header.LabourAmount = totalLabour;
        //            header.TotalWoTax = totalTaxable;
        //            header.GstAmnt = totalGst;
        //            header.IgstAmnt = totalIgst;
        //            header.NetTotal = totalNet;
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Ok(new
        //        {
        //            message = $"Synced {syncedCount} new record(s) successfully.",
        //            syncedCount
        //        });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = ex.GetBaseException().Message });
        //    }
        //}

        [HttpPost("sync")]
        public async Task<IActionResult> Sync([FromBody] SyncZeroHoursRequest? request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string? targetDealer = null;

                if (!IsSuperAdmin())
                {
                    if (string.IsNullOrWhiteSpace(GetUserDealerCode()))
                        return Unauthorized(new { message = "DealerCode claim missing." });
                    // 26.11.2026 - chosen dealer if it is one of theirs, otherwise all of theirs
                    targetDealer = GetUserInfoFromToken.ScopeDealerCode(HttpContext, request?.DealerCode);
                }
                else if (!string.IsNullOrWhiteSpace(request?.DealerCode) &&
                         !string.Equals(request.DealerCode.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    targetDealer = request.DealerCode.Trim();
                }

                var jcQuery = _context.JobCardHeaders
                    .AsNoTracking()
                    .Where(j => j.IsDelete != true);

                if (!string.IsNullOrWhiteSpace(targetDealer))
                {
                    var targets = GetUserInfoFromToken.SplitCodes(targetDealer);
                    jcQuery = jcQuery.Where(j => targets.Contains(j.DealerCode));
                }

                if (request?.FromDate.HasValue == true)
                {
                    var from = DateOnly.FromDateTime(request.FromDate.Value.Date);
                    jcQuery = jcQuery.Where(j => j.JobinDate >= from);
                }

                if (request?.ToDate.HasValue == true)
                {
                    var to = DateOnly.FromDateTime(request.ToDate.Value.Date);
                    jcQuery = jcQuery.Where(j => j.JobinDate <= to);
                }

                var jobCards = await jcQuery
                    .OrderByDescending(j => j.JobinDate)
                    .ThenByDescending(j => j.Id)
                    .ToListAsync();

                if (jobCards.Count == 0)
                {
                    await transaction.CommitAsync();
                    return Ok(new { message = "Synced 0 new record(s).", syncedCount = 0 });
                }

                var jobCardIds = jobCards.Select(j => j.Id).ToList();

                var serviceHeadMap = await _context.ServiceHeads
                    .AsNoTracking()
                    .Where(s => !string.IsNullOrWhiteSpace(s.ServiceHeadName))
                    .ToDictionaryAsync(
                        s => s.Id.ToString(),
                        s => s.ServiceHeadName!.Trim()
                    );

                var jobTypeMap = await _context.JobTypes
                    .AsNoTracking()
                    .Where(jt => !string.IsNullOrWhiteSpace(jt.JobTypeName))
                    .ToDictionaryAsync(
                        jt => jt.Id.ToString(),
                        jt => jt.JobTypeName!.Trim()
                    );

                var customersByJobId = await _context.JobCardCustomers
                    .AsNoTracking()
                    .Where(c => jobCardIds.Contains(c.JobCardHeaderId))
                    .OrderByDescending(c => c.Id)
                    .GroupBy(c => c.JobCardHeaderId)
                    .ToDictionaryAsync(g => g.Key, g => g.First());

                var locations = await _context.LocationMasters
                    .AsNoTracking()
                    .Where(l => !string.IsNullOrWhiteSpace(l.Loccode))
                    .Select(l => new { l.Dealercode, l.Loccode, l.Locname, l.State })
                    .ToListAsync();

                var locByDealerAndCode = locations
                    .Where(l => !string.IsNullOrWhiteSpace(l.Dealercode))
                    .GroupBy(l => $"{l.Dealercode!.Trim()}|{l.Loccode!.Trim()}", StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var locByCodeOnly = locations
                    .GroupBy(l => l.Loccode!.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var existingHeaders = await _context.NewZeroHoursJobHeaders
                    .Include(h => h.NewZeroHoursJobDetails)
                    .Where(h => !h.IsDelete && h.JobCardHeaderId.HasValue && jobCardIds.Contains(h.JobCardHeaderId.Value))
                    .ToListAsync();

                var existingByKey = existingHeaders
                    .GroupBy(h => $"{(h.DealerCode ?? "").Trim()}|{h.JobCardHeaderId}", StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var materialsByJobId = await _context.MaterialTransfers
                    .AsNoTracking()
                    .Where(m => !m.IsDelete && m.JobId.HasValue && jobCardIds.Contains(m.JobId.Value))
                    .GroupBy(m => m.JobId!.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

                var repairBillsByJobId = await _context.RepairBillHeaders
                    .AsNoTracking()
                    .Include(r => r.RepairBillDetails)
                        .ThenInclude(d => d.PartItem)
                    .Where(r => !r.IsDelete && r.JobId.HasValue && jobCardIds.Contains(r.JobId.Value))
                    .GroupBy(r => r.JobId!.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

                var userName = GetCurrentUserName();
                var now = DateTime.Now;
                int syncedCount = 0;

                static string ResolveDealerType(string? partyName)
                {
                    if (string.IsNullOrWhiteSpace(partyName)) return "B2C";
                    var upper = partyName.ToUpperInvariant();
                    string[] b2bKeywords = { "PVT", "LTD", "LLP", "MOTORS", "AUTO", "ENTERPRISES", "MOBILITY", "CORP", "INC", "TRADERS", "AGENCIES", "COMPANY", "CO." };
                    return b2bKeywords.Any(k => upper.Contains(k)) ? "B2B" : "B2C";
                }

                foreach (var jc in jobCards)
                {
                    var dc = (jc.DealerCode ?? targetDealer ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(dc))
                        continue;

                    var locCode = jc.Serviceloc?.Trim();
                    string? locName = null;
                    string? locState = null;

                    if (!string.IsNullOrWhiteSpace(locCode))
                    {
                        if (locByDealerAndCode.TryGetValue($"{dc}|{locCode}", out var locMatch))
                        {
                            locName = locMatch.Locname?.Trim();
                            locState = locMatch.State?.Trim();
                        }
                        else if (locByCodeOnly.TryGetValue(locCode, out var locFallback))
                        {
                            locName = locFallback.Locname?.Trim();
                            locState = locFallback.State?.Trim();
                        }
                    }

                    customersByJobId.TryGetValue(jc.Id, out var cust);

                    var key = $"{dc}|{jc.Id}";
                    existingByKey.TryGetValue(key, out var header);

                    if (header == null)
                    {
                        header = new NewZeroHoursJobHeader
                        {
                            CreatedBy = userName,
                            CreatedDate = now,
                            IsActive = true,
                            IsDelete = false
                        };
                        _context.NewZeroHoursJobHeaders.Add(header);
                        existingByKey[key] = header;
                        syncedCount++;
                    }
                    else
                    {
                        header.UpdatedBy = userName;
                        header.UpdatedDate = now;
                    }

                    var jobBills = repairBillsByJobId.TryGetValue(jc.Id, out var bills)
                        ? bills
                        : new List<RepairBillHeader>();

                    var latestBill = jobBills.OrderByDescending(b => b.Id).FirstOrDefault();
                    var partyName = cust?.CustomerName;
                    var rawServiceHead = jc.Servicehead?.ToString()?.Trim();
                    var rawJobType = jc.Jobtype?.ToString()?.Trim();

                    header.CompName = !string.IsNullOrWhiteSpace(locName) ? locName : null;
                    header.DealerState = !string.IsNullOrWhiteSpace(locState) ? locState : null;
                    header.DealerType = ResolveDealerType(partyName);
                    header.JobCardHeaderId = jc.Id;
                    header.RepairBillHeaderId = latestBill?.Id;
                    header.DealerCode = dc;
                    header.LocationCode = locCode;
                    header.LocationName = locName;
                    header.JobNo = jc.JobNo?.ToString();
                    header.JobDate = jc.JobinDate;
                    //header.InTime = jc.JobinTime?.ToString();
                    //header.CloseTime = jc.EstdelTime?.ToString();
                    header.InTime = FormatTimeHhMm(jc.JobinTime);
                    header.CloseTime = FormatTimeHhMm(jc.EstdelTime);
                    header.JobType = !string.IsNullOrWhiteSpace(rawJobType) &&
                                     jobTypeMap.TryGetValue(rawJobType, out var jtName)
                        ? jtName
                        : rawJobType;
                    header.ServiceHead = !string.IsNullOrWhiteSpace(rawServiceHead) &&
                                        serviceHeadMap.TryGetValue(rawServiceHead, out var shName)
                        ? shName
                        : rawServiceHead;
                    header.DocNo = latestBill != null
                        ? (!string.IsNullOrWhiteSpace(latestBill.Prefix)
                            ? $"{latestBill.Prefix}{latestBill.BillNo}"
                            : latestBill.BillNo.ToString())
                        : (!string.IsNullOrWhiteSpace(jc.InvoiceNo) ? jc.InvoiceNo : jc.EstNo);
                    header.InvoiceDate = latestBill?.CreatedDate.HasValue == true
                        ? DateOnly.FromDateTime(latestBill.CreatedDate.Value)
                        : null;
                    header.BrandName = string.IsNullOrWhiteSpace(header.BrandName) ? "FGWG" : header.BrandName;
                    header.KMS = jc.Vehiclekms;
                    header.CopnNo = jc.Couponno;
                    header.Supervisor = jc.Supervisor;
                    header.Technician = jc.Technician;
                    header.ExpectedDeliveryDate = jc.EstdelDate;
                    header.JobStatus = jc.JobStatus;
                    header.CreateThrough = "DMS_SYNC";

                    header.PartyName = partyName;
                    header.PartyMobile = cust?.CustomerMobile;
                    header.Model = cust?.ModelName;
                    header.ChassisNo = cust?.ChassisNo ?? jc.Chassisno;
                    header.MotorNo = cust?.MotorNo;
                    header.RegNo = cust?.RegisterNo;
                    header.SaleDate = cust?.SaleDate.HasValue == true
                        ? DateOnly.FromDateTime(cust.SaleDate.Value)
                        : null;

                    foreach (var oldDetail in header.NewZeroHoursJobDetails.Where(d => !d.IsDelete))
                    {
                        oldDetail.IsDelete = true;
                    }

                    decimal totalParts = 0m;
                    decimal totalLabour = 0m;
                    decimal totalTaxable = 0m;
                    decimal totalGst = 0m;
                    decimal totalIgst = 0m;
                    decimal totalNet = 0m;

                    if (materialsByJobId.TryGetValue(jc.Id, out var matList))
                    {
                        foreach (var mt in matList)
                        {
                            decimal qty = mt.Quantity;
                            decimal rate = mt.ItemRate;
                            decimal taxable = qty * rate;
                            decimal net = taxable;

                            totalParts += net;
                            totalTaxable += taxable;
                            totalNet += net;

                            header.NewZeroHoursJobDetails.Add(new NewZeroHoursJobDetail
                            {
                                JobCardHeaderId = jc.Id,
                                MaterialTransferId = mt.Id,
                                SourceType = "Material",
                                ItemCode = mt.ItemCode,
                                ItemDesc = mt.ItemDesc,
                                IssueType = mt.IssueType.ToString(),
                                Qty = qty,
                                Rate = rate,
                                TaxableAmount = taxable,
                                GstAmount = 0m,
                                IgstAmount = 0m,
                                NetAmount = net,
                                IsDelete = false,
                                CreatedDate = now
                            });
                        }
                    }

                    foreach (var rb in jobBills)
                    {
                        var billDetails = rb.RepairBillDetails?.ToList() ?? new List<RepairBillDetail>();
                        foreach (var rbd in billDetails)
                        {
                            var itemType = (rbd.ItemType ?? string.Empty).Trim();
                            bool isLabour = itemType.Contains("Labour", StringComparison.OrdinalIgnoreCase) ||
                                            itemType.Contains("Labor", StringComparison.OrdinalIgnoreCase) ||
                                            rbd.LabourMasterId.HasValue ||
                                            rbd.PartWiseLabourId.HasValue;

                            decimal qty = isLabour ? (rbd.LabourQty ?? 0m) : (rbd.PartQty ?? 0m);
                            decimal rate = isLabour ? (rbd.LabourRate ?? 0m) : (rbd.PartRate ?? 0m);
                            decimal taxable = isLabour
                                ? (rbd.LabourTaxblAmount ?? (qty * rate))
                                : (rbd.PartTaxblAmount ?? (qty * rate));

                            decimal gst = (rbd.Cgstamount ?? 0m) + (rbd.Sgstamount ?? 0m);
                            decimal igst = rbd.Igstamount ?? 0m;

                            decimal net = isLabour
                                ? (rbd.LabourNetAmount ?? (taxable + gst + igst))
                                : (rbd.PartNetAmount ?? (taxable + gst + igst));

                            if (isLabour)
                                totalLabour += net;
                            else
                                totalParts += net;

                            totalTaxable += taxable;
                            totalGst += gst;
                            totalIgst += igst;
                            totalNet += net;

                            header.NewZeroHoursJobDetails.Add(new NewZeroHoursJobDetail
                            {
                                JobCardHeaderId = jc.Id,
                                RepairBillId = rb.Id,
                                MaterialTransferId = rbd.MaterialId,
                                SourceType = isLabour ? "Labour" : (string.IsNullOrWhiteSpace(itemType) ? "Part" : itemType),
                                ItemCode = rbd.PartItem?.Itemcode ?? rbd.PartItemId?.ToString() ?? rbd.LabourMasterId?.ToString(),
                                ItemDesc = rbd.PartItem?.Itemdesc ?? itemType,
                                IssueType = rbd.IssutypeId?.ToString(),
                                Qty = qty,
                                Rate = rate,
                                TaxableAmount = taxable,
                                GstAmount = gst,
                                IgstAmount = igst,
                                NetAmount = net,
                                IsDelete = false,
                                CreatedDate = now
                            });
                        }
                    }

                    header.PartsAmount = totalParts;
                    header.LabourAmount = totalLabour;
                    header.TotalWoTax = totalTaxable;
                    header.GstAmnt = totalGst;
                    header.IgstAmnt = totalIgst;
                    header.NetTotal = totalNet;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = $"Synced {syncedCount} new record(s) successfully.",
                    syncedCount
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                await transaction.RollbackAsync();
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        static string? FormatTimeHhMm(object? val)
        {
            if (val == null) return null;

            if (val is TimeOnly tOnly)
                return tOnly.ToString("HH:mm");

            if (val is TimeSpan tSpan)
                return $"{(int)tSpan.TotalHours % 24:D2}:{tSpan.Minutes:D2}";

            if (val is DateTime dt)
                return dt.ToString("HH:mm");

            var raw = val.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(raw)) return null;

            if (TimeOnly.TryParse(raw, out var parsedTime))
                return parsedTime.ToString("HH:mm");

            if (DateTime.TryParse(raw, out var parsedDt))
                return parsedDt.ToString("HH:mm");

            var parts = raw.Split(':');
            if (parts.Length >= 2 &&
                int.TryParse(parts[0], out var hh) &&
                int.TryParse(parts[1], out var mm))
            {
                return $"{hh:D2}:{mm:D2}";
            }

            return raw.Length >= 5 ? raw[..5] : raw;
        }

        //[HttpGet("export")]
        //public async Task<IActionResult> Export(
        //    [FromQuery] string? DealerCode,
        //    [FromQuery] string? LocationCode,
        //    [FromQuery] DateTime? FromDate,
        //    [FromQuery] DateTime? ToDate,
        //    [FromQuery] string? Search)
        //{
        //    try
        //    {
        //        var query = BuildFilteredQuery(DealerCode, LocationCode, FromDate, ToDate, Search);

        //        var rows = await query
        //            .OrderByDescending(x => x.JobDate)
        //            .ThenByDescending(x => x.Id)
        //            .ToListAsync();

        //        static string Csv(object? val)
        //        {
        //            var s = val?.ToString() ?? string.Empty;
        //            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
        //                return $"\"{s.Replace("\"", "\"\"")}\"";
        //            return s;
        //        }

        //        var sb = new StringBuilder();
        //        sb.AppendLine(string.Join(",",
        //            "SrNo", "CompName", "DealerCode", "DealerState", "DealerType",
        //            "LocationCode", "LocationName", "JobDate", "JobNo", "DocNo",
        //            "JobStatus", "PartyName", "PartyMobile", "Model", "MotorNo",
        //            "ChassisNo", "KMS", "PartsAmount", "LabourAmount", "NetTotal", "CreateThrough"));

        //        for (int i = 0; i < rows.Count; i++)
        //        {
        //            var r = rows[i];
        //            var formattedJobDate = r.JobDate.HasValue
        //                ? r.JobDate.Value.ToString("dd-MM-yyyy")
        //                : string.Empty;

        //            sb.AppendLine(string.Join(",",
        //                Csv(i + 1),
        //                Csv(r.CompName),
        //                Csv(r.DealerCode),
        //                Csv(r.DealerState),
        //                Csv(r.DealerType),
        //                Csv(r.LocationCode),
        //                Csv(r.LocationName),
        //                Csv(formattedJobDate),
        //                Csv(r.JobNo),
        //                Csv(r.DocNo),
        //                Csv(r.JobStatus),
        //                Csv(r.PartyName),
        //                Csv(r.PartyMobile),
        //                Csv(r.Model),
        //                Csv(r.MotorNo),
        //                Csv(r.ChassisNo),
        //                Csv(r.KMS),
        //                Csv(r.PartsAmount),
        //                Csv(r.LabourAmount),
        //                Csv(r.NetTotal),
        //                Csv(r.CreateThrough)));
        //        }

        //        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        //        return File(bytes, "text/csv; charset=utf-8", "New_Zero_Hours_Report.csv");
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.GetBaseException().Message });
        //    }
        //}
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            [FromQuery] string? DealerCode,
            [FromQuery] string? LocationCode,
            [FromQuery] DateTime? FromDate,
            [FromQuery] DateTime? ToDate,
            [FromQuery] string? Search)
        { 
            try
            {
                var query = BuildFilteredQuery(DealerCode, LocationCode, FromDate, ToDate, Search);

                var rows = await query
                    .OrderByDescending(x => x.JobDate)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                static string Csv(object? val)
                {
                    var s = val?.ToString() ?? string.Empty;
                    if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                        return $"\"{s.Replace("\"", "\"\"")}\"";
                    return s;
                }

                static string FormatDate(DateOnly? d) => d.HasValue ? d.Value.ToString("dd-MM-yyyy") : string.Empty;

                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",",
                    Csv("#"),
                    Csv("Comp Name"),
                    Csv("Dealer Code"),
                    Csv("Dealer State"),
                    Csv("Job Date"),
                    Csv("Job No"),
                    Csv("Job Category"),
                    Csv("Doc No"),
                    Csv("Doc Date"),
                    Csv("Job Status"),
                    Csv("Location"),
                    Csv("Model"),
                    Csv("Brand Name"),
                    Csv("Party Name"),
                    Csv("Party Mobile"),
                    Csv("Supervisor"),
                    Csv("Technician"),
                    Csv("KMS"),
                    Csv("Service Head"),
                    Csv("Job Type"),
                    Csv("Reg No"),
                    Csv("Engine No/ Motor No"),
                    Csv("Chassis No"),
                    Csv("Expected Delivery Date"),
                    Csv("Invoice Date"),
                    Csv("Parts"),
                    Csv("Accessory"),
                    Csv("Oil"),
                    Csv("Labour"),
                    Csv("Outside Work"),
                    Csv("Total W/O Tax"),
                    Csv("GST_Amnt"),
                    Csv("IGST_Amnt"),
                    Csv("Net Total"),
                    Csv("In Time"),
                    Csv("Close Time"),
                    Csv("Close Date"),
                    Csv("Copn_No"),
                    Csv("Sale Date"),
                    Csv("Charger No"),
                    Csv("Controller No"),
                    Csv("Converter"),
                    Csv("Create Through/Portal /App"),
                    Csv("Zone Name"),
                    Csv("UniqueId"),
                    Csv("Dealer Type")));

                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    var locationDisplay = !string.IsNullOrWhiteSpace(r.LocationCode)
                        ? $"{r.LocationCode} - {r.LocationName ?? "-"}"
                        : (r.LocationName ?? string.Empty);

                    sb.AppendLine(string.Join(",",
                        Csv(i + 1),
                        Csv(r.CompName),
                        Csv(r.DealerCode),
                        Csv(r.DealerState),
                        Csv(FormatDate(r.JobDate)),
                        Csv(r.JobNo),
                        Csv(r.JobCategory),
                        Csv(r.DocNo),
                        Csv(FormatDate(r.DocDate)),
                        Csv(r.JobStatus),
                        Csv(locationDisplay),
                        Csv(r.Model),
                        Csv(r.BrandName),
                        Csv(r.PartyName),
                        Csv(r.PartyMobile),
                        Csv(r.Supervisor),
                        Csv(r.Technician),
                        Csv(r.KMS),
                        Csv(r.ServiceHead),
                        Csv(r.JobType),
                        Csv(r.RegNo),
                        Csv(r.MotorNo),
                        Csv(r.ChassisNo),
                        Csv(FormatDate(r.ExpectedDeliveryDate)),
                        Csv(FormatDate(r.InvoiceDate)),
                        Csv(r.PartsAmount),
                        Csv(0m),
                        Csv(0m),
                        Csv(r.LabourAmount),
                        Csv(0m),
                        Csv(r.TotalWoTax),
                        Csv(r.GstAmnt),
                        Csv(r.IgstAmnt),
                        Csv(r.NetTotal),
                        Csv(r.InTime),
                        Csv(r.CloseTime),
                        Csv(FormatDate(r.CloseDate)),
                        Csv(r.CopnNo),
                        Csv(FormatDate(r.SaleDate)),
                        Csv(r.ChargerNo),
                        Csv(r.ControllerNo),
                        Csv(r.ConverterNo),
                        Csv(r.CreateThrough),
                        Csv(string.Empty),
                        Csv(r.Id),
                        Csv(r.DealerType)));
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", "New_Zero_Hours_Report.csv");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var header = await _context.NewZeroHoursJobHeaders
                    .AsNoTracking()
                    .Include(x => x.NewZeroHoursJobDetails.Where(d => !d.IsDelete))
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

                if (header == null)
                    return NotFound(new { message = "Record not found." });

                if (!IsSuperAdmin())
                {
                    var userDealer = GetUserDealerCode();
                    if (!string.Equals(header.DealerCode, userDealer, StringComparison.OrdinalIgnoreCase))
                        return Forbid();
                }

                return Ok(header);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] NewZeroHoursJobHeader model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Invalid payload." });

                var dealerCode = !IsSuperAdmin()
                    ? GetUserDealerCode()
                    : (string.IsNullOrWhiteSpace(model.DealerCode) ? GetUserDealerCode() : model.DealerCode.Trim());

                if (string.IsNullOrWhiteSpace(dealerCode))
                    return BadRequest(new { message = "DealerCode is required." });

                var userName = GetCurrentUserName();
                var now = DateTime.Now;

                if (model.Id == 0)
                {
                    model.DealerCode = dealerCode;
                    model.CreatedBy = userName;
                    model.CreatedDate = now;
                    model.IsActive = true;
                    model.IsDelete = false;

                    foreach (var d in model.NewZeroHoursJobDetails)
                    {
                        d.Id = 0;
                        d.IsDelete = false;
                        d.CreatedDate = now;
                    }

                    _context.NewZeroHoursJobHeaders.Add(model);
                }
                else
                {
                    var existing = await _context.NewZeroHoursJobHeaders
                        .Include(x => x.NewZeroHoursJobDetails)
                        .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDelete);

                    if (existing == null)
                        return NotFound(new { message = "Record not found." });

                    if (!IsSuperAdmin() && !string.Equals(existing.DealerCode, GetUserDealerCode(), StringComparison.OrdinalIgnoreCase))
                        return Forbid();

                    existing.JobCardHeaderId = model.JobCardHeaderId;
                    existing.RepairBillHeaderId = model.RepairBillHeaderId;
                    existing.CompName = model.CompName;
                    existing.DealerCode = dealerCode;
                    existing.DealerState = model.DealerState;
                    existing.DealerType = model.DealerType;
                    existing.LocationCode = model.LocationCode;
                    existing.LocationName = model.LocationName;
                    existing.JobDate = model.JobDate;
                    existing.JobNo = model.JobNo;
                    existing.JobCategory = model.JobCategory;
                    existing.DocNo = model.DocNo;
                    existing.DocDate = model.DocDate;
                    existing.JobStatus = model.JobStatus;
                    existing.InvoiceDate = model.InvoiceDate;
                    existing.CloseDate = model.CloseDate;
                    existing.SaleDate = model.SaleDate;
                    existing.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
                    existing.InTime = model.InTime;
                    existing.CloseTime = model.CloseTime;
                    existing.Model = model.Model;
                    existing.BrandName = model.BrandName;
                    existing.PartyName = model.PartyName;
                    existing.PartyMobile = model.PartyMobile;
                    existing.Supervisor = model.Supervisor;
                    existing.Technician = model.Technician;
                    existing.KMS = model.KMS;
                    existing.ServiceHead = model.ServiceHead;
                    existing.JobType = model.JobType;
                    existing.RegNo = model.RegNo;
                    existing.MotorNo = model.MotorNo;
                    existing.ChassisNo = model.ChassisNo;
                    existing.CopnNo = model.CopnNo;
                    existing.ChargerNo = model.ChargerNo;
                    existing.ControllerNo = model.ControllerNo;
                    existing.ConverterNo = model.ConverterNo;
                    existing.CreateThrough = model.CreateThrough;
                    existing.EstimatedJobExpenses = model.EstimatedJobExpenses;
                    existing.PartsAmount = model.PartsAmount;
                    existing.LabourAmount = model.LabourAmount;
                    existing.TotalWoTax = model.TotalWoTax;
                    existing.GstAmnt = model.GstAmnt;
                    existing.IgstAmnt = model.IgstAmnt;
                    existing.NetTotal = model.NetTotal;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedBy = userName;
                    existing.UpdatedDate = now;

                    foreach (var oldDetail in existing.NewZeroHoursJobDetails.Where(d => !d.IsDelete))
                        oldDetail.IsDelete = true;

                    foreach (var newDetail in model.NewZeroHoursJobDetails)
                    {
                        existing.NewZeroHoursJobDetails.Add(new NewZeroHoursJobDetail
                        {
                            JobCardHeaderId = newDetail.JobCardHeaderId,
                            RepairBillId = newDetail.RepairBillId,
                            MaterialTransferId = newDetail.MaterialTransferId,
                            SourceType = newDetail.SourceType,
                            ItemCode = newDetail.ItemCode,
                            ItemDesc = newDetail.ItemDesc,
                            IssueType = newDetail.IssueType,
                            Qty = newDetail.Qty,
                            Rate = newDetail.Rate,
                            TaxableAmount = newDetail.TaxableAmount,
                            GstAmount = newDetail.GstAmount,
                            IgstAmount = newDetail.IgstAmount,
                            NetAmount = newDetail.NetAmount,
                            IsDelete = false,
                            CreatedDate = now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Saved successfully.", id = model.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _context.NewZeroHoursJobHeaders
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

                if (existing == null)
                    return NotFound(new { message = "Record not found." });

                if (!IsSuperAdmin() && !string.Equals(existing.DealerCode, GetUserDealerCode(), StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                existing.IsDelete = true;
                existing.UpdatedBy = GetCurrentUserName();
                existing.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }
    }
}