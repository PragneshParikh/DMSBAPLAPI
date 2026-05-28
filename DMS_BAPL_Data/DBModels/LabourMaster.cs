using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class LabourMaster
{
    public int Id { get; set; }

    public string? LabourCode { get; set; }

    public string? LabourDescription { get; set; }

    public string? ModelCode { get; set; }

    public int? ModelCc { get; set; }

    public int? CityTier { get; set; }

    public decimal? LabourRate { get; set; }

    public decimal? Sgst { get; set; }

    public decimal? Cgst { get; set; }

    public decimal? Igst { get; set; }

    public string? Category { get; set; }

    public string? Hsncode { get; set; }

    public string? Oemmodelname { get; set; }

    public bool? IsLabourActive { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
