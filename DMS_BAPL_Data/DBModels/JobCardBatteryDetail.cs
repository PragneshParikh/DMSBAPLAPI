using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class JobCardBatteryDetail
{
    public int Id { get; set; }

    public int JobCardHeaderId { get; set; }

    public string? BatteryMake { get; set; }

    public string? BatterySerialNo { get; set; }

    public string? BatteryOcv { get; set; }

    public string? BatteryCcv { get; set; }

    public string? BatteryDischarge { get; set; }

    public string? BatteryCapacityAh { get; set; }

    public string? BatteryVoltage { get; set; }

    public string? MotorDrawing { get; set; }

    public string? ChargerMake { get; set; }

    public string? ChargerNo { get; set; }

    public string? ConverterNo { get; set; }

    public string? ControllerNo { get; set; }

    public string? BatteryChemical { get; set; }

    public string? BatteryCapacity { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public virtual JobCardHeader JobCardHeader { get; set; } = null!;
}
