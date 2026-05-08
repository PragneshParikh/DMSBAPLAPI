using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class InvoiceHeader
{
    public int Id { get; set; }

    public string InvoiceType { get; set; } = null!;

    public string ServiceType { get; set; } = null!;

    public string DocumentNo { get; set; } = null!;

    public int? ReferenceId { get; set; }

    public bool? IsFinalized { get; set; }

    public int? CustomerId { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public string? Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? DealerCode { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
