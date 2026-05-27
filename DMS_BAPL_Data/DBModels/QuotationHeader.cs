using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class QuotationHeader
{
    public int Id { get; set; }

    public string QuotationNo { get; set; } = null!;

    public DateTime? QuotationDate { get; set; }

    public int? CustomerLedgerId { get; set; }

    public string? SalesExecutive { get; set; }

    public string? Reference { get; set; }

    public string? QuotationType { get; set; }

    public int? FinancerLedgerId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual LedgerMaster? CustomerLedger { get; set; }

    public virtual LedgerMaster? FinancerLedger { get; set; }

    public virtual ICollection<QuotationDetail> QuotationDetails { get; set; } = new List<QuotationDetail>();
}
