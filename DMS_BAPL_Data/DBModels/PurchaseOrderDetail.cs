using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class PurchaseOrderDetail
{
    public int Id { get; set; }

    public int Ponumber { get; set; }

    public int ItemCode { get; set; }

    public int? Qty { get; set; }

    public string? Unit { get; set; }

    public decimal? Rate { get; set; }

    public decimal? LineAmount { get; set; }

    public bool Status { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public decimal? Subsidy { get; set; }

    public int? LineNumber { get; set; }

    public virtual ItemMaster ItemCodeNavigation { get; set; } = null!;

    public virtual PurchaseOrder PonumberNavigation { get; set; } = null!;
}
