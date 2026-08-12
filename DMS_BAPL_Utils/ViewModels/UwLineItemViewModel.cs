using System;
using System.Collections.Generic;
namespace DMS_BAPL_Utils.ViewModels
{
    public class UwLineItemListViewModel
    {
        // --- existing fields, unchanged ---
        public int Id { get; set; }
        public int WarrantyJcclaimId { get; set; }
        public int WarrantyJcclaimDetailId { get; set; }
        public string? ClaimPrefix { get; set; }
        public int? ClaimNo { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string? ChassisNo { get; set; }
        public string? SupplierName { get; set; }
        public string? JobCardNo { get; set; }
        public DateTime? JobCardDate { get; set; }
        public string? DealerCompanyName { get; set; }
        public string? LocationName { get; set; }
        public string? ItemType { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? Hsn { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal GstAmount { get; set; }
        public decimal Amount { get; set; }
        public string? DeviceInward { get; set; }
        public string? DeviceOutward { get; set; }
        public string? ServiceType { get; set; }
        public DateTime? FailureDate { get; set; }
        public string? ServiceHead { get; set; }
        public string? MotorNo { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? Kms { get; set; }
        public string? CustomerName { get; set; }
        public string? MobileNumber { get; set; }
        public DateTime? SaleDate { get; set; }
        public string? ClaimAccount { get; set; }
        public string? PartCode { get; set; }
        public string? PartDescription { get; set; }
        public string? LabourCode { get; set; }
        public string? LabourDescription { get; set; }
        public string? ClaimType { get; set; }
        public string? DealerObservation { get; set; }
        public string? Rca { get; set; }

        // --- NEW: added per explicit request ---
        // Mrp comes directly from WarrantyJcclaimDetail.Mrp (confirmed
        // real on the entity, wasn't populated before this).
        public decimal Mrp { get; set; }

        // Cgst/Sgst/Igst amounts come from the already-fetched
        // RepairBillDetail (rbd.Cgstamount/Sgstamount/Igstamount) - same
        // confirmed source GenerateWarrantyJCClaimPdf already uses, just
        // not previously surfaced on this ViewModel.
        public decimal Cgst { get; set; }
        public decimal Sgst { get; set; }
        public decimal Igst { get; set; }

        // Percentages, per explicit request - same confirmed source as
        // the amounts above: rbd.PartItem.Cgst/Sgst/Igst for parts,
        // rbd.LabourMaster.Cgst/Sgst/Igst for labour (exact pattern
        // GenerateWarrantyJCClaimPdf already uses).
        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal IgstPercent { get; set; }
    }
    public class UwLineItemSearchViewModel
    {
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? ClaimNo { get; set; }
        public string? Status { get; set; } // null/empty = all statuses
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
    public class UwLineItemSearchResultViewModel
    {
        public List<UwLineItemListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
    // Payload for both Approve and Reject actions - RejectionReason only
    // meaningful when rejecting, ignored on approve.
    public class UwLineItemActionViewModel
    {
        public int Id { get; set; } // UwLineItem's own Id
        public string? RejectionReason { get; set; }
    }
}