using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class WarrantyJcclaimDetail
{
    public int Id { get; set; }

    public int WarrantyJcclaimHeaderId { get; set; }

    public int RepairBillDetailId { get; set; }

    public string? ItemType { get; set; }

    public int? MaterialId { get; set; }

    public int? LabourMasterId { get; set; }

    public int? PartWiseLabourId { get; set; }

    public int? PartItemId { get; set; }

    public decimal? Qty { get; set; }

    public decimal? Rate { get; set; }

    public decimal? Amount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? ClaimType { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? DealerObservation { get; set; }

    public string? RootCauseAnalysis { get; set; }

    public virtual RepairBillDetail RepairBillDetail { get; set; } = null!;

    public virtual WarrantyJcclaim WarrantyJcclaimHeader { get; set; } = null!;
}
