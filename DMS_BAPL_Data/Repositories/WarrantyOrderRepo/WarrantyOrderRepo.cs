using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.WarrantyOrderRepo
{
    public class WarrantyOrderRepo : IWarrantyOrderRepo
    {
        private readonly BapldmsvadContext _context;

        public WarrantyOrderRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<int> InsertWarrantyOrder(WarrantyOrderViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = new WarrantyOrder
                {
                    DealerCode = model.DealerCode,
                    DateFrom = model.DateFrom!.Value,
                    DateTo = model.DateTo!.Value,
                    BatchNo = model.BatchNo!,
                    BatchDate = model.BatchDate!.Value,
                    OrderNo = model.OrderNo!,
                    OrderDate = model.OrderDate!.Value,
                    Location = model.Location!,
                    ClaimType = model.ClaimType!,
                    SupplierId = model.SupplierId!.Value,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsApproved = model.IsApproved
                };

                _context.WarrantyOrders.Add(header);
                await _context.SaveChangesAsync();

                if (model.WarrantyClaimIds != null && model.WarrantyClaimIds.Any())
                {
                    var details = model.WarrantyClaimIds.Select(claimId => new WarrantyOrderDetail
                    {
                        WarrantyOrderHeaderId = header.Id,
                        WarrantyJcclaimId = claimId,
                        IsApproved = model.ClaimApprovals?.FirstOrDefault(a => a.ClaimId == claimId)?.IsApproved ?? false,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    _context.WarrantyOrderDetails.AddRange(details);
                    await _context.SaveChangesAsync();

                    // Snapshot fully-resolved grid rows now, while everything
                    // is known-good - reading the grid later never needs to
                    // re-join JobCardHeader/RepairBillHeader/etc again.
                    foreach (var claimId in model.WarrantyClaimIds)
                    {
                        await SnapshotClaimGridRows(header.Id, claimId, userId);
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return header.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateWarrantyOrder(WarrantyOrderViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = await _context.WarrantyOrders
                    .Include(x => x.WarrantyOrderDetails)
                    .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsActive);

                if (header == null)
                    return false;

                header.DateFrom = model.DateFrom!.Value;
                header.DateTo = model.DateTo!.Value;
                header.BatchNo = model.BatchNo!;
                header.BatchDate = model.BatchDate!.Value;
                header.OrderNo = model.OrderNo!;
                header.OrderDate = model.OrderDate!.Value;
                header.Location = model.Location!;
                header.ClaimType = model.ClaimType!;
                header.SupplierId = model.SupplierId!.Value;
                header.IsApproved = model.IsApproved;
                header.UpdatedBy = userId;
                header.UpdatedDate = DateTime.Now;

                // Replace the claim links wholesale - simplest correct approach
                // for a batch editor where the claim list can be re-selected.
                _context.WarrantyOrderDetails.RemoveRange(header.WarrantyOrderDetails);

                // Also clear the old grid snapshot - it'll be rebuilt below
                // for whatever the new claim list ends up being.
                var oldGridRows = await _context.WarrantyOrderGridDetails
                    .Where(g => g.WarrantyOrderHeaderId == header.Id)
                    .ToListAsync();
                _context.WarrantyOrderGridDetails.RemoveRange(oldGridRows);

                if (model.WarrantyClaimIds != null && model.WarrantyClaimIds.Any())
                {
                    var newDetails = model.WarrantyClaimIds.Select(claimId => new WarrantyOrderDetail
                    {
                        WarrantyOrderHeaderId = header.Id,
                        WarrantyJcclaimId = claimId,
                        IsApproved = model.ClaimApprovals?.FirstOrDefault(a => a.ClaimId == claimId)?.IsApproved ?? false,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    await _context.WarrantyOrderDetails.AddRangeAsync(newDetails);
                    await _context.SaveChangesAsync();

                    foreach (var claimId in model.WarrantyClaimIds)
                    {
                        await SnapshotClaimGridRows(header.Id, claimId, userId);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteWarrantyOrder(int id, string userId)
        {
            var header = await _context.WarrantyOrders
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (header == null)
                return false;

            // Soft delete - keeps this row (and everything referencing it:
            // WarrantyOrderDetail, WarrantyOrderGridDetail) fully intact in
            // the DB. Only SearchWarrantyOrders (the List page) filters on
            // IsActive, so this order disappears from that list specifically
            // while remaining fully viewable via GetWarrantyOrderById (which
            // does NOT filter on IsActive) if navigated to directly.
            header.IsActive = false;
            header.UpdatedBy = userId;
            header.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WarrantyOrderViewModel?> GetWarrantyOrderById(int id)
        {
            // No IsActive filter here, deliberately - a soft-deleted order
            // (removed only from the List page's search results) should
            // still be fully viewable if navigated to directly, along with
            // its WarrantyOrderGridDetail snapshot, which is untouched
            // either way since DeleteWarrantyOrder no longer removes rows.
            var header = await _context.WarrantyOrders
                .Include(x => x.WarrantyOrderDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (header == null)
                return null;

            // Read straight from the snapshot - no live joins, so display is
            // immune to any future changes/mismatches in the source data.
            var gridRows = await _context.WarrantyOrderGridDetails
                .Where(g => g.WarrantyOrderHeaderId == id)
                .ToListAsync();

            // Per-claim approval lives on WarrantyOrderDetail (the link table),
            // not the grid snapshot - merged in below by claim id.
            var approvalByClaimId = header.WarrantyOrderDetails
                .ToDictionary(d => d.WarrantyJcclaimId, d => d.IsApproved);

            var claims = gridRows
                .GroupBy(g => g.WarrantyJcclaimId)
                .Select(grp =>
                {
                    var first = grp.First();
                    return new WarrantyJCClaimFullViewModel
                    {
                        Id = grp.Key,
                        // Snapshot stores the already-combined "{Prefix}{Number}"
                        // display string in ClaimNo (see SnapshotClaimGridRows) -
                        // put it in ClaimPrefix and leave ClaimNo null, since the
                        // frontend just concatenates both back together anyway.
                        ClaimPrefix = first.ClaimNo,
                        ClaimNo = null,
                        ClaimDate = first.ClaimDate,
                        JobCardNo = first.JobCardNo,
                        JobCardDate = first.JobCardDate,
                        InvoiceNo = first.InvoiceNo,
                        InvoiceDate = first.InvoiceDate,
                        ServiceHead = first.ServiceHead,
                        Kms = first.Kms,
                        LocationName = first.LocationName,
                        ChassisNo = first.ChassisNo,
                        MotorNo = first.MotorNo,
                        PartyName = first.PartyName,
                        IsApproved = approvalByClaimId.TryGetValue(grp.Key, out var approved) && approved,
                        Details = grp
                            .Where(g => g.ItemType != null) // skip the empty-details placeholder row
                            .Select(g => new WarrantyJCClaimDetailLineViewModel
                            {
                                ItemType = g.ItemType,
                                PartCode = g.PartCode,
                                PartName = g.PartName,
                                PartDescription = g.PartDescription,
                                LabourCode = g.LabourCode,
                                LabourDescription = g.LabourDescription,
                                Quantity = g.Quantity ?? 0,
                                CgstPercent = g.CgstPercent ?? 0,
                                CgstAmount = g.CgstAmount ?? 0,
                                SgstPercent = g.SgstPercent ?? 0,
                                SgstAmount = g.SgstAmount ?? 0,
                                IgstPercent = g.IgstPercent ?? 0,
                                IgstAmount = g.IgstAmount ?? 0,
                                TotalAmount = g.TotalAmount ?? 0,
                                Mrp = g.Mrp
                            }).ToList()
                    };
                })
                .ToList();

            // Resolved unscoped - header.Location can legitimately belong to
            // a different dealer than the one viewing this order (the exact
            // gap that left the Location dropdown blank: it was only ever
            // searched within the current user's own dealer's locations).
            var headerLocationName = !string.IsNullOrWhiteSpace(header.Location)
                ? await _context.LocationMasters
                    .Where(l => l.Loccode == header.Location)
                    .Select(l => l.Locname)
                    .FirstOrDefaultAsync()
                : null;

            return new WarrantyOrderViewModel
            {
                Id = header.Id,
                DealerCode = header.DealerCode,
                DateFrom = header.DateFrom,
                DateTo = header.DateTo,
                BatchNo = header.BatchNo,
                BatchDate = header.BatchDate,
                OrderNo = header.OrderNo,
                OrderDate = header.OrderDate,
                Location = header.Location,
                LocationName = headerLocationName,
                ClaimType = header.ClaimType,
                SupplierId = header.SupplierId,
                IsApproved = header.IsApproved,
                IsActive = header.IsActive,
                WarrantyClaimIds = header.WarrantyOrderDetails.Select(d => d.WarrantyJcclaimId).ToList(),
                Claims = claims
            };
        }

        public async Task<WarrantyOrderSearchResultViewModel> SearchWarrantyOrders(WarrantyOrderSearchViewModel filter)
        {
            var query = _context.WarrantyOrders
                .Include(x => x.WarrantyOrderDetails)
                .AsQueryable();

            // Default (IncludeInactive = false) keeps the current behavior -
            // the List page never sees soft-deleted orders. Only the form
            // page's "show the latest order" fallback passes true, so that
            // view can still find an order after it's been deleted from the
            // List specifically.
            if (!filter.IncludeInactive)
                query = query.Where(x => x.IsActive);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.OrderDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.OrderDate <= filter.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.BatchNo))
                query = query.Where(x => x.BatchNo.Contains(filter.BatchNo));

            if (filter.BatchDate.HasValue)
                query = query.Where(x => x.BatchDate.Date == filter.BatchDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.OrderNo))
                query = query.Where(x => x.OrderNo.Contains(filter.OrderNo));

            if (filter.OrderDate.HasValue)
                query = query.Where(x => x.OrderDate.Date == filter.OrderDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(x => x.Location == filter.Location);

            if (!string.IsNullOrWhiteSpace(filter.ClaimType))
                query = query.Where(x => x.ClaimType == filter.ClaimType);

            if (filter.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == filter.SupplierId.Value);

            if (filter.IsApproved.HasValue)
                query = query.Where(x => x.IsApproved == filter.IsApproved.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WarrantyOrderListViewModel
                {
                    Id = x.Id,
                    BatchNo = x.BatchNo,
                    BatchDate = x.BatchDate,
                    OrderNo = x.OrderNo,
                    OrderDate = x.OrderDate,
                    Location = x.Location,
                    ClaimType = x.ClaimType,
                    SupplierId = x.SupplierId,
                    TotalClaims = x.WarrantyOrderDetails.Count,
                    TotalMrp = _context.WarrantyOrderGridDetails
                        .Where(g => g.WarrantyOrderHeaderId == x.Id)
                        .Sum(g => (decimal?)g.Mrp) ?? 0,
                    IsApproved = x.IsApproved,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            // Temporary result wrapper - see note on WarrantyOrderSearchResultViewModel.
            return new WarrantyOrderSearchResultViewModel
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<NextOrderNumberViewModel> GetNextOrderNumbers(string dealerCode)
        {
            // Indian financial year: April -> March. Aug 2026 => FY "26-27".
            var today = DateTime.Now;
            int fyStartYear = today.Month >= 4 ? today.Year : today.Year - 1;
            int fyEndYear = fyStartYear + 1;
            string fySuffix = $"{fyStartYear % 100:D2}-{fyEndYear % 100:D2}";
            string batchSuffix = $"BT/{fySuffix}";

            // Batch No format: "{seq}/BT/26-27" - seq resets each FY since the
            // suffix changes, scoped per dealer.
            var existingBatchNos = await _context.WarrantyOrders
                .Where(x => x.DealerCode == dealerCode && x.BatchNo.EndsWith("/" + batchSuffix))
                .Select(x => x.BatchNo)
                .ToListAsync();

            int maxBatchSeq = 0;
            foreach (var b in existingBatchNos)
            {
                var numPart = b.Split('/')[0];
                if (int.TryParse(numPart, out int seq) && seq > maxBatchSeq)
                    maxBatchSeq = seq;
            }
            string nextBatchNo = $"{maxBatchSeq + 1}/{batchSuffix}";

            // Order No: plain running integer starting at 1, scoped per dealer.
            var existingOrderNos = await _context.WarrantyOrders
                .Where(x => x.DealerCode == dealerCode)
                .Select(x => x.OrderNo)
                .ToListAsync();

            int maxOrderSeq = 0;
            foreach (var o in existingOrderNos)
            {
                if (int.TryParse(o, out int seq) && seq > maxOrderSeq)
                    maxOrderSeq = seq;
            }
            string nextOrderNo = (maxOrderSeq + 1).ToString();

            return new NextOrderNumberViewModel
            {
                BatchNo = nextBatchNo,
                OrderNo = nextOrderNo
            };
        }

        public async Task<WarrantyJCClaimFullViewModel?> GetWarrantyJCClaimById(int id)
        {
            return await BuildClaimFullViewModel(id);
        }

        // Extracted so both the live-lookup endpoint (GetWarrantyJCClaimById,
        // used while a claim is still pending/unsaved) and the save-time
        // snapshot (SnapshotClaimGridRows) share exactly one resolution path.
        private async Task<WarrantyJCClaimFullViewModel?> BuildClaimFullViewModel(int id)
        {
            var claim = await _context.WarrantyJcclaims
                .Include(x => x.Supplier)
                .Include(x => x.JobCardHeader)
                    .ThenInclude(jc => jc.ServiceheadNavigation)
                .Include(x => x.RepairBillHeader)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.PartItem)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.LabourMaster)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.PartWiseLabour)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null)
                return null;

            // Motor No - same lookup JobCardRepo.GetIssueTypebasedJobDetail uses
            // for the original Warranty JobCard Claim screen: latest
            // ChassisBatteryDetail row for this chassis.
            var motorNo = await _context.ChassisBatteryDetails
                .Where(x => x.ChassisNo == claim.ChassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.MotorNo)
                .FirstOrDefaultAsync();

            // Location name - same join key (Serviceloc = Loccode) the Job
            // Search on Warranty JobCard Claim already resolves successfully.
            // No locareaidno filter here - that's for dropdown options only,
            // not for identifying what this job's actual location is.
            var locationName = claim.JobCardHeader != null
                ? await _context.LocationMasters
                    .Where(l => l.Loccode == claim.JobCardHeader.Serviceloc)
                    .Select(l => l.Locname)
                    .FirstOrDefaultAsync()
                : null;

            return new WarrantyJCClaimFullViewModel
            {
                Id = claim.Id,
                ClaimPrefix = claim.ClaimPrefix,
                ClaimNo = claim.ClaimNo,
                ClaimDate = claim.ClaimDate,
                ChassisNo = claim.ChassisNo,

                JobCardNo = claim.JobCardHeader != null
                    ? $"{claim.JobCardHeader.Jobprefix}{claim.JobCardHeader.JobNo}"
                    : null,
                JobCardDate = claim.JobCardHeader?.JobinDate?.ToDateTime(TimeOnly.MinValue),

                // Defaulting to the repair bill as "Invoice" - see note in
                // WarrantyJCClaimFullViewModel.cs if JobCardHeader.InvoiceNo
                // (the original vehicle-sale invoice) is what's actually meant.
                InvoiceNo = claim.RepairBillHeader != null
                    ? $"{claim.RepairBillHeader.Prefix}{claim.RepairBillHeader.BillNo}"
                    : null,
                InvoiceDate = claim.RepairBillHeader?.CreatedDate,

                ServiceHead = claim.JobCardHeader?.ServiceheadNavigation?.ServiceHeadName,
                Kms = claim.JobCardHeader?.Vehiclekms,

                MotorNo = motorNo,
                PartyName = claim.Supplier?.LedgerName,

                SupplierId = claim.SupplierId,
                ServiceLocation = claim.JobCardHeader?.Serviceloc,
                LocationName = locationName,

                Details = claim.WarrantyJcclaimDetails.Select(d =>
                {
                    bool isLabour = d.ItemType == "Labour";
                    var rbd = d.RepairBillDetail;

                    // Tax % display still comes from the master (Cgst/Sgst/Igst percent
                    // aren't stored on WarrantyJcclaimDetail), but the actual monetary
                    // values (Rate, Amount, GST amount, Total, Mrp) MUST come from the
                    // saved WarrantyJcclaimDetail row (d) - that's the data the user
                    // actually entered/validated when saving the claim, not whatever the
                    // original repair bill line happens to show today.

                    return new WarrantyJCClaimDetailLineViewModel
                    {
                        Id = d.Id,
                        ItemType = d.ItemType,

                        PartCode = !isLabour ? rbd?.PartItem?.Itemcode : null,
                        PartName = !isLabour ? rbd?.PartItem?.Itemname : null,
                        PartDescription = !isLabour ? rbd?.PartItem?.Itemdesc : null,

                        LabourCode = isLabour
                            ? (rbd?.LabourMaster?.LabourCode ?? rbd?.PartWiseLabour?.LabourCode)
                            : null,
                        LabourDescription = isLabour
                            ? (rbd?.LabourMaster?.LabourDescription ?? rbd?.PartWiseLabour?.LabourName)
                            : null,

                        Quantity = d.Qty ?? 0,
                        Rate = d.Rate ?? 0, 

                        CgstPercent = isLabour
                            ? (rbd?.LabourMaster?.Cgst ?? rbd?.PartWiseLabour?.Cgst ?? 0)
                            : (rbd?.PartItem?.Cgst ?? 0),
                        SgstPercent = isLabour
                            ? (rbd?.LabourMaster?.Sgst ?? rbd?.PartWiseLabour?.Sgst ?? 0)
                            : (rbd?.PartItem?.Sgst ?? 0),
                        IgstPercent = isLabour
                            ? (rbd?.LabourMaster?.Igst ?? rbd?.PartWiseLabour?.Igst ?? 0)
                            : (rbd?.PartItem?.Igst ?? 0),
                        CgstAmount = d.ItemType != null && d.TaxAmount.HasValue ? 0 : 0, 
                        SgstAmount = 0,                                                  
                        IgstAmount = d.TaxAmount ?? 0,

                        TotalAmount = d.TotalAmount ?? 0,
                        Amount = d.Amount ?? 0,     
                        Mrp = d.Mrp ?? 0,           

                        DealerObservation = d.DealerObservation,
                        RootCauseAnalysis = d.RootCauseAnalysis
                    };
                }).ToList()
            };
        }

        // Resolves a claim once via BuildClaimFullViewModel, then persists
        // its fully-resolved data into WarrantyOrderGridDetail - one row per
        // part/labour line, or a single placeholder row (ItemType = null) if
        // the claim has no line items, so it still appears in the grid.
        private async Task SnapshotClaimGridRows(int orderHeaderId, int claimId, string userId)
        {
            var claim = await BuildClaimFullViewModel(claimId);
            if (claim == null)
                return;

            var claimNoDisplay = $"{claim.ClaimPrefix}{claim.ClaimNo}";

            if (claim.Details == null || !claim.Details.Any())
            {
                _context.WarrantyOrderGridDetails.Add(new WarrantyOrderGridDetail
                {
                    WarrantyOrderHeaderId = orderHeaderId,
                    WarrantyJcclaimId = claimId,
                    ClaimNo = claimNoDisplay,
                    ClaimDate = claim.ClaimDate,
                    JobCardNo = claim.JobCardNo,
                    JobCardDate = claim.JobCardDate,
                    InvoiceNo = claim.InvoiceNo,
                    InvoiceDate = claim.InvoiceDate,
                    ServiceHead = claim.ServiceHead,
                    Kms = claim.Kms,
                    LocationName = claim.LocationName,
                    ChassisNo = claim.ChassisNo,
                    MotorNo = claim.MotorNo,
                    PartyName = claim.PartyName,
                    ItemType = null, // placeholder - no line items on this claim
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                });
                return;
            }

            foreach (var line in claim.Details)
            {
                _context.WarrantyOrderGridDetails.Add(new WarrantyOrderGridDetail
                {
                    WarrantyOrderHeaderId = orderHeaderId,
                    WarrantyJcclaimId = claimId,
                    ClaimNo = claimNoDisplay,
                    ClaimDate = claim.ClaimDate,
                    JobCardNo = claim.JobCardNo,
                    JobCardDate = claim.JobCardDate,
                    InvoiceNo = claim.InvoiceNo,
                    InvoiceDate = claim.InvoiceDate,
                    ServiceHead = claim.ServiceHead,
                    Kms = claim.Kms,
                    LocationName = claim.LocationName,
                    ChassisNo = claim.ChassisNo,
                    MotorNo = claim.MotorNo,
                    PartyName = claim.PartyName,
                    ItemType = line.ItemType,
                    PartName = line.PartName,
                    PartDescription = line.PartDescription,
                    PartCode = line.PartCode,
                    LabourCode = line.LabourCode,
                    LabourDescription = line.LabourDescription,
                    Quantity = line.Quantity,
                    CgstPercent = line.CgstPercent,
                    CgstAmount = line.CgstAmount,
                    SgstPercent = line.SgstPercent,
                    SgstAmount = line.SgstAmount,
                    IgstPercent = line.IgstPercent,
                    IgstAmount = line.IgstAmount,
                    TotalAmount = line.TotalAmount,
                    Mrp = line.Mrp,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                });
            }
        }

        public async Task<byte[]> GenerateWarrantyOrderPdf(int id)
        {
            var order = await GetWarrantyOrderById(id);
            if (order == null)
                throw new InvalidOperationException($"Warranty Order with Id {id} not found.");

            var claims = order.Claims ?? new List<WarrantyJCClaimFullViewModel>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(7));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Warranty Order").FontSize(14).Bold();
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"Batch No: {order.BatchNo}");
                            row.RelativeItem().Text($"Batch Date: {order.BatchDate:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Order No: {order.OrderNo}");
                            row.RelativeItem().Text($"Order Date: {order.OrderDate:dd-MM-yyyy}");
                        });
                        col.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Text($"Date From: {order.DateFrom:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Date To: {order.DateTo:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Location: {order.Location}");
                            row.RelativeItem().Text($"Claim Type: {order.ClaimType}");
                        });
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(20);   // Sr.No
                            columns.RelativeColumn(1.3f); // Claim No/Date
                            columns.RelativeColumn(1.3f); // JobCard No/Date
                            columns.RelativeColumn(1.3f); // Invoice No/Date
                            columns.RelativeColumn(1.1f); // Service Head/KMS
                            columns.RelativeColumn(1.2f); // Location Name
                            columns.RelativeColumn(1.3f); // Chassis No
                            columns.RelativeColumn(1.1f); // Motor No
                            columns.RelativeColumn(1.4f); // Party
                            columns.RelativeColumn(1.1f); // Part Name
                            columns.RelativeColumn(1.3f); // Part Description
                            columns.RelativeColumn(0.9f); // Part Code
                            columns.RelativeColumn(0.9f); // Labor Code
                            columns.RelativeColumn(1.2f); // Labor Description
                            columns.ConstantColumn(30);   // Qty
                            columns.RelativeColumn(0.9f); // CGST
                            columns.RelativeColumn(0.9f); // SGST
                            columns.RelativeColumn(0.9f); // IGST
                            columns.RelativeColumn(1.0f); // Total
                        });

                        void HeaderCell(QuestPDF.Infrastructure.IContainer c, string text) =>
                            c.Background(Colors.Grey.Lighten2).Padding(2).Text(text).Bold();

                        table.Header(header =>
                        {
                            HeaderCell(header.Cell(), "Sr.No");
                            HeaderCell(header.Cell(), "Claim No / Date");
                            HeaderCell(header.Cell(), "JobCard No / Date");
                            HeaderCell(header.Cell(), "Invoice No / Date");
                            HeaderCell(header.Cell(), "Service Head / KMS");
                            HeaderCell(header.Cell(), "Location Name");
                            HeaderCell(header.Cell(), "Chassis No");
                            HeaderCell(header.Cell(), "Motor No");
                            HeaderCell(header.Cell(), "Party");
                            HeaderCell(header.Cell(), "Part Name");
                            HeaderCell(header.Cell(), "Part Description");
                            HeaderCell(header.Cell(), "Part Code");
                            HeaderCell(header.Cell(), "Labor Code");
                            HeaderCell(header.Cell(), "Labor Description");
                            HeaderCell(header.Cell(), "Qty");
                            HeaderCell(header.Cell(), "CGST");
                            HeaderCell(header.Cell(), "SGST");
                            HeaderCell(header.Cell(), "IGST");
                            HeaderCell(header.Cell(), "Total Amount");
                        });

                        int srNo = 1;
                        decimal grandTotal = 0;

                        foreach (var claim in claims)
                        {
                            var lines = (claim.Details != null && claim.Details.Any())
                                ? claim.Details
                                : new List<WarrantyJCClaimDetailLineViewModel> { new WarrantyJCClaimDetailLineViewModel() };

                            foreach (var line in lines)
                            {
                                grandTotal += line.TotalAmount;

                                table.Cell().Padding(2).Text(srNo++.ToString());
                                table.Cell().Padding(2).Text($"{claim.ClaimPrefix}{claim.ClaimNo}\n{claim.ClaimDate:dd-MM-yyyy}");
                                table.Cell().Padding(2).Text($"{claim.JobCardNo}\n{claim.JobCardDate:dd-MM-yyyy}");
                                table.Cell().Padding(2).Text($"{claim.InvoiceNo}\n{claim.InvoiceDate:dd-MM-yyyy}");
                                table.Cell().Padding(2).Text($"{claim.ServiceHead}\n{claim.Kms} km");
                                table.Cell().Padding(2).Text(claim.LocationName);
                                table.Cell().Padding(2).Text(claim.ChassisNo);
                                table.Cell().Padding(2).Text(claim.MotorNo);
                                table.Cell().Padding(2).Text(claim.PartyName);
                                table.Cell().Padding(2).Text(line.PartName);
                                table.Cell().Padding(2).Text(line.PartDescription);
                                table.Cell().Padding(2).Text(line.PartCode);
                                table.Cell().Padding(2).Text(line.LabourCode);
                                table.Cell().Padding(2).Text(line.LabourDescription);
                                table.Cell().Padding(2).AlignRight().Text(line.Quantity.ToString());
                                table.Cell().Padding(2).AlignRight().Text($"{line.CgstPercent}%\n{line.CgstAmount:0.00}");
                                table.Cell().Padding(2).AlignRight().Text($"{line.SgstPercent}%\n{line.SgstAmount:0.00}");
                                table.Cell().Padding(2).AlignRight().Text($"{line.IgstPercent}%\n{line.IgstAmount:0.00}");
                                table.Cell().Padding(2).AlignRight().Text(line.TotalAmount.ToString("0.00"));
                            }
                        }

                        // Grand total row
                        table.Cell().ColumnSpan(18).Padding(2).AlignRight().Text("Grand Total:").Bold();
                        table.Cell().Padding(2).AlignRight().Text(grandTotal.ToString("0.00")).Bold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}