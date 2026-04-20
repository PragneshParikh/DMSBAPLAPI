using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ModelwiseServiceSchedule
{
    public int Id { get; set; }

    public int OemmodelId { get; set; }

    public string? SrNo { get; set; }

    public int DaysFrom { get; set; }

    public int DaysTo { get; set; }

    public int JourneyFrom { get; set; }

    public int JourneyTo { get; set; }

    public string? ServiceFrom { get; set; }

    public string? ServiceHead { get; set; }

    public string? ServiceType { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
