using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class HsnwiseTaxCode
{
    public int Id { get; set; }

    public string Hsncode { get; set; } = null!;

    public string AtaxCode { get; set; } = null!;

    public string StateFlag { get; set; } = null!;

    public DateTime EffectiveDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
