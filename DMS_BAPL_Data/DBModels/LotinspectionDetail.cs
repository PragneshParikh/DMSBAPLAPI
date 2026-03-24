using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class LotinspectionDetail
{
    public int Id { get; set; }

    public int LotHeaderId { get; set; }

    public string? ModelName { get; set; }

    public int? NoofVehicle { get; set; }

    public string? ChassisNo { get; set; }

    public string? MotorNo { get; set; }

    public string? BatteryNo { get; set; }

    public string? ChargerNo { get; set; }

    public int? KeyFobSetQty { get; set; }

    public int? ChargerQty { get; set; }

    public int? MirrorsetQty { get; set; }

    public int? FirstaidkitQty { get; set; }

    public int? ToolKitQty { get; set; }

    public int? OwnersManual { get; set; }

    public int? IgnitionKeyset { get; set; }

    public int? AttributeCard { get; set; }

    public int? ChargingKit { get; set; }

    public DateOnly? InspectionDate { get; set; }

    public string? VehicleStatus { get; set; }

    public string? DamageDetails { get; set; }

    public string? ChassisWiseRemarks { get; set; }

    public string? LocationName { get; set; }

    public string? LotVehicleDamageImage { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateOnly CreatedDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public DateOnly UpdatedDate { get; set; }

    public virtual LotinspectionHeader LotHeader { get; set; } = null!;
}
