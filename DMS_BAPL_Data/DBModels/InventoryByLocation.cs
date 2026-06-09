using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class InventoryByLocation
{
    public int Id { get; set; }

    public string ItemCode { get; set; } = null!;

    public string ChassisNo { get; set; } = null!;

    public string LocationCode { get; set; } = null!;

    public string DealerCode { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
