using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class OemmodelWarranty
{
    public int Id { get; set; }

    public int OemmodelId { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public decimal? Odoreading { get; set; }

    public string? DurationType { get; set; }

    public decimal? Duration { get; set; }

    public bool? IsB2b { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual OemmodelMaster Oemmodel { get; set; } = null!;
}
