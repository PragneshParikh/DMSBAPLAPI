using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class Hsrporder
{
    public int Id { get; set; }

    public string OrderNo { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public int? SupplierLedgerId { get; set; }

    public string? InvoiceNo { get; set; }

    public string? ChassisNo { get; set; }

    public string? RegNo { get; set; }

    public string? SaleBillNo { get; set; }

    public int? SaleBillDetailsId { get; set; }

    public int? CustomerLedgerId { get; set; }

    public bool? IsFrontPlate { get; set; }

    public bool? IsRearPlate { get; set; }

    public bool? IsTlpsticker { get; set; }

    public string? Hsrpstatus { get; set; }

    public string? Hsrpresponse { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual LedgerMaster? SupplierLedger { get; set; }
}
