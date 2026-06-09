using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class VehicleStockTransferDetail
{
    public int Id { get; set; }

    public int TransferHeaderId { get; set; }

    public string ChassisNo { get; set; } = null!;

    public string ItemCode { get; set; } = null!;

    public decimal ItemRate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual VehicleStockTransferHeader TransferHeader { get; set; } = null!;
}
