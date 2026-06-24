using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ChassisBatteryDetail
{
    public int Id { get; set; }

    public string ChassisNo { get; set; } = null!;

    public int? MotorOrderNo { get; set; }

    public string? MotorNo { get; set; }

    public int? BatteryOrderNo { get; set; }

    public string? BatteryNo { get; set; }

    public int? ChargerOrderNo { get; set; }

    public string? ChargerNo { get; set; }

    public int? ControllerOrderNo { get; set; }

    public string? ControllerNo { get; set; }

    public int? ConverterOrderNo { get; set; }

    public string? ConverterNo { get; set; }

    public string? BatteryChemical { get; set; }

    public string? BatteryCapacity { get; set; }

    public string? BatteryMake { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
