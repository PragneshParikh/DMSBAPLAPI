using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class TaxCodeMaster
{
    public int Id { get; set; }

    public string TaxCode { get; set; } = null!;

    public string? Description { get; set; }

    public decimal TaxRate { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
