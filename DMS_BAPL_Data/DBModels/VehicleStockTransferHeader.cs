using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class VehicleStockTransferHeader
{
    public int Id { get; set; }

    public string TransferNo { get; set; } = null!;

    public DateTime TransferDate { get; set; }

    public string IssuingLocationCode { get; set; } = null!;

    public string IssuingStaffCode { get; set; } = null!;

    public string ReceivingLocationCode { get; set; } = null!;

    public string ReceivingStaffCode { get; set; } = null!;

    public string? Remarks { get; set; }

    public decimal TransferTotal { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? DealerCode { get; set; }

    public virtual ICollection<VehicleStockTransferDetail> VehicleStockTransferDetails { get; set; } = new List<VehicleStockTransferDetail>();
}
