using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class BatteryCapacityMaster
{
    public int Id { get; set; }

    public string BatteryCapacity { get; set; } = null!;

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
