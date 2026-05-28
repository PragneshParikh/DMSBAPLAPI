using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class PartWiseLabourMaster
{
    public int Id { get; set; }

    public string? LabourCode { get; set; }

    public string? LabourName { get; set; }

    public string? PartCode { get; set; }

    public string? PartDescription { get; set; }

    public string? ModelName { get; set; }

    public int? CityTier { get; set; }

    public decimal? LabourRate { get; set; }

    public decimal? LabourHrs { get; set; }

    public decimal? Cgst { get; set; }

    public decimal? Sgst { get; set; }

    public decimal? Igst { get; set; }

    public string? JobType { get; set; }

    public string? DealerCode { get; set; }

    public string? Hsncode { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsActive { get; set; }
}
