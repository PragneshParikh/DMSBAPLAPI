using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class AggregateTaxCode
{
    public int Id { get; set; }

    public string AtaxCode { get; set; } = null!;

    public string? Description { get; set; }

    public int SrNo { get; set; }

    public int TaxCode { get; set; }

    public decimal TaxRate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
