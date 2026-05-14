using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class FfirdetailObservation
{
    public int Id { get; set; }

    public int Ffirid { get; set; }

    public string? ObservationFailedParts { get; set; }

    public string? RootCauseofFailure { get; set; }

    public string? CorrectiveAction { get; set; }

    public string? ResolutionComplaint { get; set; }

    public string? PresentStatusofVehicle { get; set; }

    public string? VehicleOffRoadReason { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Ffirheader Ffir { get; set; } = null!;
}
