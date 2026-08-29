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
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }
        public string? ItemType { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }

        // --- NEW: pre-formatted display strings for ActionDate ---
        // ActionDate is the single timestamp column both ApproveUwLineItem
        // and RejectUwLineItem write to (DMS_BAPL_Data.Repositories.
        // UwLineItemRepo) — there is no separate ApprovedDate/RejectedDate
        // column in the database. These two fields serve BOTH the
        // "Approved"/"Rejected" sub-display in the Actions column and the
        // "Updated Date:/Time:" column, formatted server-side so the client
        // just displays the string as-is rather than re-parsing/re-piping a
        // raw DateTime value (which was getting its time component stripped
        // somewhere client-side before render — see the frontend
        // investigation; this sidesteps that entirely instead of depending
        // on finding it).
        public string? ActionDateDisplay { get; set; } // e.g. "25-08-2026"
        public string? ActionTimeDisplay { get; set; } // e.g. "14:48:04"

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

        // --- Mrp / Cgst / Sgst / Igst amounts + percentages ---
        public decimal Mrp { get; set; }
        public decimal Cgst { get; set; }
        public decimal Sgst { get; set; }
        public decimal Igst { get; set; }
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