using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ChassisBatteryDetail
{
    public int Id { get; set; }

    public string ChassisNo { get; set; } = null!;

    public string? MotorNo { get; set; }

    public string? BatteryNo { get; set; }

    public string? ChargerNo { get; set; }

    public string? ControllerNo { get; set; }

    public string? BatteryCapacity { get; set; }

    public string? BatteryMake { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
