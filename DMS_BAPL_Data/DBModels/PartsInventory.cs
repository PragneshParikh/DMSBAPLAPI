using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class PartsInventory
{
    public int Id { get; set; }

    public string TransId { get; set; } = null!;

    public string ItemCode { get; set; } = null!;

    public string VoucherNo { get; set; } = null!;

    public string TransType { get; set; } = null!;

    public string? BatchNo { get; set; }

    public int BatchOpeningQty { get; set; }

    public int BatchTransQty { get; set; }

    public int BatchClosingQty { get; set; }

    public DateOnly TransDate { get; set; }

    public string DealerLocation { get; set; } = null!;

    public string? VendorCode { get; set; }

    public string FinalStockFlag { get; set; } = null!;

    public decimal TotalRate { get; set; }

    public decimal PurchaseRate { get; set; }

    public string Potype { get; set; } = null!;

    public int? PostTransaction { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
