using System;

namespace DMS_BAPL_Data.DBModels
{
    // Stores one row per resolved grid line (claim + part/labour line),
    // snapshotted at the moment a Warranty Order is saved. Reading the grid
    // means selecting straight from this table - no live joins across
    // JobCardHeader/RepairBillHeader/RepairBillDetail/LocationMaster/etc,
    // so display is immune to any future join/mismatch issues.
    public partial class WarrantyOrderGridDetail
    {
        public int Id { get; set; }
        public int WarrantyOrderHeaderId { get; set; }
        public int WarrantyJcclaimId { get; set; }

        public string? ClaimNo { get; set; }
        public DateTime? ClaimDate { get; set; }

        public string? JobCardNo { get; set; }
        public DateTime? JobCardDate { get; set; }

        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public string? ServiceHead { get; set; }
        public decimal? Kms { get; set; }

        public string? LocationName { get; set; }
        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? PartyName { get; set; }

        public string? ItemType { get; set; }
        public string? PartName { get; set; }
        public string? PartDescription { get; set; }
        public string? PartCode { get; set; }
        public string? LabourCode { get; set; }
        public string? LabourDescription { get; set; }

        public decimal? Quantity { get; set; }
        public decimal? CgstPercent { get; set; }
        public decimal? CgstAmount { get; set; }
        public decimal? SgstPercent { get; set; }
        public decimal? SgstAmount { get; set; }
        public decimal? IgstPercent { get; set; }
        public decimal? IgstAmount { get; set; }
        public decimal? TotalAmount { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual WarrantyOrder WarrantyOrderHeader { get; set; } = null!;
    }
}