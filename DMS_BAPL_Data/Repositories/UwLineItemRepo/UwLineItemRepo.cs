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
            // CREATE WARRANTY ORDER
            // ============================================================

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

            var orderId = await _warrantyOrderRepo.InsertWarrantyOrder(orderModel, userId ?? "system");

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
            var lineItem = await _context.UwLineItems.FirstOrDefaultAsync(u => u.Id == model.Id);

            if (lineItem == null)
            {
                return (
                    false,
                    $"UW Line Item with Id {model.Id} not found."
                );
            }


            lineItem.Status = "Rejected";
            lineItem.RejectionReason = model.RejectionReason;
            lineItem.ActionBy = userId;
            lineItem.ActionDate = DateTime.Now;


            await _context.SaveChangesAsync();
            return (
                true, null
            );
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