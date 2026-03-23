using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class Lotinspection
{
    public int Id { get; set; }

    public string? InvoiceNo { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    public int? Lotno { get; set; }

    public DateOnly? ArrivalDate { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public string? Lrno { get; set; }

    public DateOnly? Lrdate { get; set; }

    public string? TruckNo { get; set; }

    public string? TransporterName { get; set; }

    public string? DriverName { get; set; }

    public string? DriverContact { get; set; }

    public string? CommonReMarks { get; set; }

    public string? VehicleFasteningBracket { get; set; }

    public string? PlasticCover { get; set; }

    public string? NameSupervisor { get; set; }

    public string? ModelName { get; set; }

    public int? NoofVehicle { get; set; }

    public string? ChassisNo { get; set; }

    public string? MotorNo { get; set; }

    public string? BatteryNo { get; set; }

    public string? ChargerNo { get; set; }

    public int? NoofKeyFobSet { get; set; }

    public int? NoofCharger { get; set; }

    public int? NoofMirrorset { get; set; }

    public int? NoofFirstaidkit { get; set; }

    public int? NoofToolKit { get; set; }

    public DateOnly? InspectionDate { get; set; }

    public string? VehicleStatus { get; set; }

    public string? DamageDetails { get; set; }

    public string? ChassisWiseRemarks { get; set; }

    public string? LocationName { get; set; }

    public int? OwnersManual { get; set; }

    public int? IgnitionKeyset { get; set; }

    public int? AttributeCard { get; set; }

    public int? ChargingKit { get; set; }

    public string? LotVehicleDamageImage { get; set; }

    public string? CreatedBy { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateOnly? UpdatedDate { get; set; }
}
