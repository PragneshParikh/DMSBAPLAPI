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

    public int? JobType { get; set; }

    public string? DealerCode { get; set; }

    public string? Hsncode { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public int? ServiceType { get; set; }

    public int? ServiceHead { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<RepairBillDetail> RepairBillDetails { get; set; } = new List<RepairBillDetail>();

    public virtual ServiceHead? ServiceHeadNavigation { get; set; }

    public virtual ServiceType? ServiceTypeNavigation { get; set; }
}
