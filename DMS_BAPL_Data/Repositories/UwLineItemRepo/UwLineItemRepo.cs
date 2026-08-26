using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.WarrantyOrderRepo;
using DMS_BAPL_Data.Repositories.WarrantyInvoiceRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.UwLineItemRepo
{
    public class UwLineItemRepo : IUwLineItemRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IWarrantyOrderRepo _warrantyOrderRepo;
        private readonly IWarrantyInvoiceRepo _warrantyInvoiceRepo;

        public UwLineItemRepo(
            BapldmsvadContext context,
            IWarrantyOrderRepo warrantyOrderRepo,
            IWarrantyInvoiceRepo warrantyInvoiceRepo)
        {
            _context = context;
            _warrantyOrderRepo = warrantyOrderRepo;
            _warrantyInvoiceRepo = warrantyInvoiceRepo;
        }

        // Called from InsertWarrantyJCClaim, right after the claim itself
        // is saved - this is the "once JCClaim submitted it will reflect
        // in UW-Line Items window" behavior, per explicit request.
        public async Task InsertUwLineItem(int warrantyJcclaimId, string? userId)
        {
            _context.UwLineItems.Add(new UwLineItem
            {
                WarrantyJcclaimId = warrantyJcclaimId,
                Status = "Pending",
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        public async Task<UwLineItemSearchResultViewModel> SearchUwLineItems(UwLineItemSearchViewModel filter)
        {
            var query = from d in _context.WarrantyJcclaimDetails
                        .Include(x => x.RepairBillDetail).ThenInclude(rb => rb.LabourMaster)
                        .Include(x => x.RepairBillDetail).ThenInclude(rb => rb.PartItem)
                        join c in _context.WarrantyJcclaims on d.WarrantyJcclaimHeaderId equals c.Id
                        join u in _context.UwLineItems on c.Id equals u.WarrantyJcclaimId
                        select new { d, c, u };

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.c.DealerCode == filter.DealerCode);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.c.ClaimDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.c.ClaimDate <= filter.DateTo.Value);

            if (filter.ClaimNo.HasValue)
                query = query.Where(x => x.c.ClaimNo == filter.ClaimNo.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(x => x.u.Status == filter.Status);

            query = query.OrderByDescending(x => x.u.CreatedDate).ThenBy(x => x.d.Id);

            var totalCount = await query.CountAsync();

            var rows = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = new List<UwLineItemListViewModel>();

            foreach (var row in rows)
            {
                var d = row.d;
                var c = row.c;
                var u = row.u;

                bool isLabour = d.ItemType == "Labour";

                var rbd = await _context.RepairBillDetails
                         .Include(r => r.LabourMaster)
                         .Include(r => r.PartItem)
                         .FirstOrDefaultAsync(r => r.Id == d.RepairBillDetailId);

                var jobCardHeader = c.JobCardHeaderId.HasValue
                    ? await _context.JobCardHeaders
                        .Include(j => j.ServiceheadNavigation)
                        .FirstOrDefaultAsync(j => j.Id == c.JobCardHeaderId.Value)
                    : null;

                var supplier = c.SupplierId.HasValue
                    ? await _context.LedgerMasters.FirstOrDefaultAsync(l => l.Id == c.SupplierId.Value)
                    : null;

                var dealerCompanyName = !string.IsNullOrWhiteSpace(c.DealerCode)
                    ? await _context.DealerMasters
                        .Where(dm => dm.Dealercode == c.DealerCode)
                        .Select(dm => dm.Compname)
                        .FirstOrDefaultAsync()
                    : null;

                var customerLedger = c.CustomerLedgerId.HasValue
                    ? await _context.LedgerMasters.FirstOrDefaultAsync(l => l.Id == c.CustomerLedgerId.Value)
                    : null;

                var repairBillHeader = c.RepairBillHeaderId.HasValue
                    ? await _context.RepairBillHeaders.FirstOrDefaultAsync(r => r.Id == c.RepairBillHeaderId.Value)
                    : null;

                var chassisBattery = await _context.ChassisBatteryDetails
                    .Where(x => x.ChassisNo == c.ChassisNo)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

                var chassisDetail = await _context.ChassisDetails
                    .FirstOrDefaultAsync(x => x.ChassisNo == c.ChassisNo);

                var jobCardNo = jobCardHeader != null ? $"{jobCardHeader.Jobprefix}{jobCardHeader.JobNo}" : null;
                var invoiceNo = repairBillHeader != null ? $"{repairBillHeader.Prefix}{repairBillHeader.BillNo}" : null;

                // ── Rate / tax calculation ──────────────────────────────────
                // FIX: previously GstAmount/TotalAmount were read straight off
                // WarrantyJcclaimDetail.TaxAmount/TotalAmount, which were
                // computed ONCE at claim-submission time
                // (WarrantyJobCardClaimRepo.InsertWarrantyJCClaim) from a
                // single incoming "IgstAmount" DTO field. For an intrastate
                // repair (real IGST = 0, CGST+SGST apply instead), if the
                // claim form didn't apply the same IGST-absorbs-CGST+SGST
                // fallback used elsewhere in this codebase
                // (WarrantyOrderRepo.BuildClaimFullViewModel), that stored
                // TaxAmount/TotalAmount silently drops the CGST+SGST tax
                // entirely — showing an "Amount" that's too low with no way
                // to recover the missing tax from that field.
                //
                // Fix: compute Total Gst / Total Amount from the SAME real
                // RepairBillDetail.Cgstamount/Sgstamount/Igstamount already
                // used for the Cgst/Sgst/Igst columns below, so the grid is
                // always internally consistent — Total Gst always equals
                // Cgst + Sgst + Igst, and Amount always equals the taxable
                // value plus that real total.
                decimal cgstAmount = rbd?.Cgstamount ?? 0;
                decimal sgstAmount = rbd?.Sgstamount ?? 0;
                decimal igstAmount = rbd?.Igstamount ?? 0;
                decimal totalGstAmount = cgstAmount + sgstAmount + igstAmount;

                decimal baseAmount = d.Amount ?? 0;
                decimal lineTotalAmount = baseAmount + totalGstAmount;
                // ─────────────────────────────────────────────────────────────

                items.Add(new UwLineItemListViewModel
                {
                    Id = u.Id,
                    WarrantyJcclaimId = c.Id,
                    WarrantyJcclaimDetailId = d.Id,

                    ClaimPrefix = c.ClaimPrefix,
                    ClaimNo = c.ClaimNo,
                    ClaimDate = c.ClaimDate,
                    ChassisNo = c.ChassisNo,
                    SupplierName = supplier?.LedgerName,
                    JobCardNo = jobCardNo,
                    JobCardDate = jobCardHeader?.CreatedDate,

                    DealerCompanyName = dealerCompanyName,
                    LocationCode = c.LocationCode,
                    LocationName = c.LocationName,

                    ItemType = d.ItemType,
                    ItemCode = isLabour ? rbd?.LabourMaster?.LabourCode : rbd?.PartItem?.Itemcode,
                    ItemDescription = isLabour ? rbd?.LabourMaster?.LabourDescription : rbd?.PartItem?.Itemdesc,

                    TotalAmount = lineTotalAmount,

                    Status = u.Status,
                    RejectionReason = u.RejectionReason,
                    ActionBy = u.ActionBy,
                    ActionDate = u.ActionDate,

                    // Pre-formatted server-side so the client displays these
                    // as plain strings — see UwLineItemListViewModel for why.
                    ActionDateDisplay = u.ActionDate.HasValue ? u.ActionDate.Value.ToString("dd-MM-yyyy") : null,
                    ActionTimeDisplay = u.ActionDate.HasValue ? u.ActionDate.Value.ToString("HH:mm:ss") : null,
                    Hsn = isLabour ? rbd?.LabourMaster?.Hsncode : rbd?.PartItem?.Hsncode,

                    Qty = d.Qty ?? 0,
                    Rate = d.Rate ?? 0,
                    GstAmount = totalGstAmount,
                    Amount = baseAmount,

                    Mrp = d.Mrp ?? 0,
                    Cgst = cgstAmount,
                    Sgst = sgstAmount,
                    Igst = igstAmount,

                    CgstPercent = isLabour ? (rbd?.LabourMaster?.Cgst ?? 0) : (rbd?.PartItem?.Cgst ?? 0),
                    SgstPercent = isLabour ? (rbd?.LabourMaster?.Sgst ?? 0) : (rbd?.PartItem?.Sgst ?? 0),
                    IgstPercent = isLabour ? (rbd?.LabourMaster?.Igst ?? 0) : (rbd?.PartItem?.Igst ?? 0),

                    DeviceInward = null,
                    DeviceOutward = null,
                    ServiceType = null,
                    FailureDate = null,

                    ServiceHead = jobCardHeader?.ServiceheadNavigation?.ServiceHeadName,
                    MotorNo = chassisBattery?.MotorNo,
                    InvoiceNo = invoiceNo,
                    InvoiceDate = repairBillHeader?.CreatedDate,
                    Kms = jobCardHeader?.Vehiclekms,

                    CustomerName = customerLedger?.LedgerName,
                    MobileNumber = customerLedger?.MobileNumber,

                    SaleDate = chassisDetail?.SaleDate,
                    ClaimAccount = c.ClaimAccount,

                    PartCode = !isLabour ? rbd?.PartItem?.Itemcode : null,
                    PartDescription = !isLabour ? rbd?.PartItem?.Itemdesc : null,
                    LabourCode = isLabour ? rbd?.LabourMaster?.LabourCode : null,
                    LabourDescription = isLabour ? rbd?.LabourMaster?.LabourDescription : null,
                    ClaimType = d.ClaimType,
                    DealerObservation = d.DealerObservation,
                    Rca = d.RootCauseAnalysis,
                });
            }

            return new UwLineItemSearchResultViewModel
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> ApproveUwLineItem(UwLineItemActionViewModel model, string? userId)
        {
            var lineItem = await _context.UwLineItems
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (lineItem == null)
            {
                return (
                    false,
                    $"UW Line Item with Id {model.Id} not found."
                );
            }

            var claim = await _context.WarrantyJcclaims
                .FirstOrDefaultAsync(
                    c => c.Id == lineItem.WarrantyJcclaimId);

            if (claim == null)
            {
                return (
                    false,
                    $"Warranty Claim with Id {lineItem.WarrantyJcclaimId} not found."
                );
            }

            if (lineItem.Status == "Approved")
            {
                return (
                    false,
                    "This UW Line Item is already approved."
                );
            }


            // ============================================================
            // CLAIM / ORDER INFORMATION
            // ============================================================

            var jobCardHeader = claim.JobCardHeaderId.HasValue
                ? await _context.JobCardHeaders
                    .FirstOrDefaultAsync(
                        j => j.Id == claim.JobCardHeaderId.Value)
                : null;

            var serviceLocation =
                jobCardHeader?.Serviceloc;

            var dealerCode =
                claim.DealerCode ?? "";

            var today =
                DateTime.Now;

            // ============================================================
            // CREATE OR REUSE WARRANTY ORDER
            // ============================================================
            // FIX: previously this always called InsertWarrantyOrder,
            // creating a brand-new WarrantyOrder row every single time —
            // including when re-approving a claim that had been rejected
            // before. RejectUwLineItem only deactivates the existing order
            // (IsActive = false) via DeactivateDownstreamRecordsIfNoActive
            // ClaimsAsync above; it never deletes the WarrantyOrderDetail
            // link. So now: if this claim already has an order linked to
            // it, that SAME order is reactivated and refreshed instead of a
            // new one being inserted. A brand-new order is only created the
            // first time a given claim is ever approved.
            var existingOrderDetail = await _context.WarrantyOrderDetails
                .FirstOrDefaultAsync(od => od.WarrantyJcclaimId == claim.Id);

            int orderId;

            if (existingOrderDetail != null &&
                await _context.WarrantyOrders.AnyAsync(o => o.Id == existingOrderDetail.WarrantyOrderHeaderId))
            {
                var existingOrder = await _context.WarrantyOrders
                    .FirstAsync(o => o.Id == existingOrderDetail.WarrantyOrderHeaderId);

                // Reactivate + refresh the existing order rather than
                // inserting a new one. BatchNo/OrderNo/BatchDate/OrderDate
                // are deliberately left untouched — this is the SAME order
                // document coming back into use, not a new one, so its
                // original numbering and creation date are preserved.
                existingOrder.IsActive = true;
                existingOrder.DateFrom = today;
                existingOrder.DateTo = today;
                existingOrder.Location = serviceLocation ?? existingOrder.Location;
                existingOrder.SupplierId = claim.SupplierId ?? existingOrder.SupplierId;
                existingOrder.IsApproved = false; // needs its own separate approval step again
                existingOrder.UpdatedBy = userId ?? "system";
                existingOrder.UpdatedDate = today;

                existingOrderDetail.IsApproved = false;

                orderId = existingOrder.Id;
            }
            else
            {
                var orderNumbers = await _warrantyOrderRepo.GetNextOrderNumbers(dealerCode);

                var orderModel = new WarrantyOrderViewModel
                {
                    Id = 0,
                    DealerCode = dealerCode,
                    DateFrom = today,
                    DateTo = today,
                    BatchNo = orderNumbers.BatchNo,
                    BatchDate = today,
                    OrderNo = orderNumbers.OrderNo,
                    OrderDate = today,
                    Location = serviceLocation,
                    ClaimType = "Warranty",
                    SupplierId = claim.SupplierId,
                    IsApproved = false,
                    WarrantyClaimIds = new List<int> { claim.Id },
                    ClaimApprovals = new List<ClaimApprovalViewModel>
                    {
                        new ClaimApprovalViewModel { ClaimId = claim.Id, IsApproved = false }
                    }
                };

                orderId = await _warrantyOrderRepo.InsertWarrantyOrder(orderModel, userId ?? "system");
            }

            // ============================================================
            // UPDATE UW LINE ITEM
            // ============================================================

            lineItem.Status = "Approved";
            lineItem.RejectionReason = null;
            lineItem.ActionBy = userId;
            lineItem.ActionDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return (true, null);

            // ============================================================
            // CREATE WARRANTY INVOICE
            // ============================================================

            var invoiceNumbers =
                await _warrantyInvoiceRepo
                    .GetNextInvoiceNumbers(dealerCode);


            var invoiceModel =
                new WarrantyInvoiceViewModel
                {
                    Id = 0,

                    DealerCode =
                        dealerCode,

                    DateFrom =
                        today,

                    DateTo =
                        today,

                    BatchNo =
                        invoiceNumbers.BatchNo,

                    BatchDate =
                        today,

                    InvoicePrefix =
                        invoiceNumbers.InvoicePrefix,

                    InvoiceNo =
                        invoiceNumbers.InvoiceNo,

                    InvoiceDate =
                        today,

                    ClaimType =
                        "Warranty",

                    SupplierId =
                        claim.SupplierId,

                    IsApproved =
                        false,

                    WarrantyOrderIds =
                        new List<int>
                        {
                    orderId
                        },

                    OrderApprovals =
                        new List<OrderApprovalViewModel>
                        {
                    new OrderApprovalViewModel
                    {
                        OrderId =
                            orderId,

                        IsApproved =
                            false
                    }
                        }
                };


            await _warrantyInvoiceRepo
                .InsertWarrantyInvoice(
                    invoiceModel,
                    userId ?? "system");


            // ============================================================
            // UPDATE UW LINE ITEM
            // ============================================================

            lineItem.Status = "Approved";
            lineItem.RejectionReason = null;
            lineItem.ActionBy = userId;
            lineItem.ActionDate = DateTime.Now;


            await _context.SaveChangesAsync();


            return (
                true,
                null
            );
        }
        public async Task<(bool Success, string? ErrorMessage)> RejectUwLineItem(UwLineItemActionViewModel model, string? userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var lineItem = await _context.UwLineItems.FirstOrDefaultAsync(u => u.Id == model.Id);

                if (lineItem == null)
                {
                    return (
                        false,
                        $"UW Line Item with Id {model.Id} not found."
                    );
                }

                bool wasApproved = lineItem.Status == "Approved";

                lineItem.Status = "Rejected";
                lineItem.RejectionReason = model.RejectionReason;
                lineItem.ActionBy = userId;
                lineItem.ActionDate = DateTime.Now;

                await _context.SaveChangesAsync();

                // Per explicit request: rejecting a claim that had already
                // been approved — and therefore already has an
                // Order/Invoice/Packing Slip generated from it — now cleans
                // up that downstream data instead of leaving it dangling.
                if (wasApproved)
                {
                    await DeactivateDownstreamRecordsIfNoActiveClaimsAsync(lineItem.WarrantyJcclaimId, userId);
                }

                await transaction.CommitAsync();

                return (
                    true, null
                );
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // After a claim is rejected, deactivates (IsActive = false) any
        // Order/Invoice/Packing Slip that claim was the LAST remaining
        // ACTIVE claim/order on.
        //
        // Deliberately does NOT touch or delete any WarrantyOrderDetail /
        // WarrantyOrderGridDetail / WarrantyInvoiceDetail /
        // WarrantyInvoiceGridDetail / WarrantyPackingSlipDetail link or
        // snapshot rows — those links are kept fully intact so that if this
        // same claim is approved again later, ApproveUwLineItem can find and
        // REACTIVATE that same order instead of creating a duplicate (see
        // the "CREATE OR REUSE WARRANTY ORDER" block there).
        //
        // SAFETY: an Order can bundle multiple claims and an Invoice can
        // bundle multiple Orders via the general Warranty Order/Invoice
        // management screens (not just this UW Line Item flow) — so a
        // header is only deactivated once NONE of its linked claims/orders
        // are still Approved/active. A record still legitimately shared
        // with another active claim is left completely untouched.
        // ════════════════════════════════════════════════════════════════════
        private async Task DeactivateDownstreamRecordsIfNoActiveClaimsAsync(int claimId, string? userId)
        {
            var now = DateTime.Now;
            var actingUser = userId ?? "system";

            var orderDetails = await _context.WarrantyOrderDetails
                .Where(od => od.WarrantyJcclaimId == claimId)
                .ToListAsync();

            foreach (var orderDetail in orderDetails)
            {
                var orderHeaderId = orderDetail.WarrantyOrderHeaderId;

                var otherActiveClaimExists = await (
                    from od in _context.WarrantyOrderDetails
                    join u in _context.UwLineItems on od.WarrantyJcclaimId equals u.WarrantyJcclaimId
                    where od.WarrantyOrderHeaderId == orderHeaderId
                          && od.WarrantyJcclaimId != claimId
                          && u.Status == "Approved"
                    select od.Id
                ).AnyAsync();

                if (otherActiveClaimExists)
                    continue; // order is still legitimately in use for another claim - leave it alone

                var orderHeader = await _context.WarrantyOrders.FirstOrDefaultAsync(o => o.Id == orderHeaderId);
                if (orderHeader != null && orderHeader.IsActive)
                {
                    orderHeader.IsActive = false;
                    orderHeader.UpdatedBy = actingUser;
                    orderHeader.UpdatedDate = now;
                }

                var invoiceDetails = await _context.WarrantyInvoiceDetails
                    .Where(id => id.WarrantyOrderHeaderId == orderHeaderId)
                    .ToListAsync();

                foreach (var invoiceDetail in invoiceDetails)
                {
                    var invoiceHeaderId = invoiceDetail.WarrantyInvoiceHeaderId;

                    var otherActiveOrderExists = await (
                        from id2 in _context.WarrantyInvoiceDetails
                        join o2 in _context.WarrantyOrders on id2.WarrantyOrderHeaderId equals o2.Id
                        where id2.WarrantyInvoiceHeaderId == invoiceHeaderId
                              && id2.WarrantyOrderHeaderId != orderHeaderId
                              && o2.IsActive
                        select id2.Id
                    ).AnyAsync();

                    if (otherActiveOrderExists)
                        continue;

                    var invoiceHeader = await _context.WarrantyInvoices.FirstOrDefaultAsync(i => i.Id == invoiceHeaderId);
                    if (invoiceHeader != null && invoiceHeader.IsActive)
                    {
                        invoiceHeader.IsActive = false;
                        invoiceHeader.UpdatedBy = actingUser;
                        invoiceHeader.UpdatedDate = now;
                    }

                    var packingSlips = await _context.WarrantyPackingSlips
                        .Where(s => s.WarrantyInvoiceHeaderId == invoiceHeaderId && s.IsActive)
                        .ToListAsync();
                    foreach (var slip in packingSlips)
                    {
                        slip.IsActive = false;
                        slip.UpdatedBy = actingUser;
                        slip.UpdatedDate = now;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUwLineItem(int id)
        {
            var lineItem = await _context.UwLineItems
                .FirstOrDefaultAsync(u => u.Id == id);

            if (lineItem == null)
                return (false, $"UW Line Item with Id {id} not found.");
            if (lineItem.Status == "Approved")
                return (false, "This line item is already approved and has an Order/Invoice generated from it. It cannot be deleted directly - reject it first if it needs to be reversed.");

            _context.UwLineItems.Remove(lineItem);
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}