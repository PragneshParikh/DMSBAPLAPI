using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class RepairBillHeader
{
    public int Id { get; set; }

    public string LocationCode { get; set; } = null!;

    public string Prefix { get; set; } = null!;

    public int BillNo { get; set; }

    public string BillType { get; set; } = null!;

    public int? CashAccount { get; set; }

    public string? PartyName { get; set; }

    public string? MobileNumber { get; set; }

    public int? Scheme { get; set; }

    public int JobId { get; set; }

    public int InsuranceId { get; set; }

    public string? InsDecription { get; set; }

    public string? SurveyorName { get; set; }

    public int? ContactNumber { get; set; }

    public string? PolicyNo { get; set; }

    public DateTime? InsValidTill { get; set; }

    public bool? ZeroDepo { get; set; }

    public string? Remarks { get; set; }

    public decimal? TotalDiscount { get; set; }

    public decimal? TotalTaxableAmount { get; set; }

    public decimal? TotalNetAmount { get; set; }

    public decimal? AmountReceived { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual LedgerMaster Insurance { get; set; } = null!;

    public virtual JobCardHeader Job { get; set; } = null!;

    public virtual ICollection<RepairBillDetail> RepairBillDetails { get; set; } = new List<RepairBillDetail>();
}
