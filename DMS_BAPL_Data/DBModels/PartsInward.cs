using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class PartsInward
{
    public int Id { get; set; }

    public DateTime InvoiceDate { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public string PartNo { get; set; } = null!;

    public int ItemIdno { get; set; }

    public string ItemHsncode { get; set; } = null!;

    public decimal ItemRate { get; set; }

    public decimal ItemMrp { get; set; }

    public int ItemQty { get; set; }

    public decimal Sgst { get; set; }

    public decimal Cgst { get; set; }

    public decimal Igst { get; set; }

    public decimal Ugst { get; set; }

    public decimal ItemDisc { get; set; }

    public string DiscountType { get; set; } = null!;

    public string LocCode { get; set; } = null!;

    public string DealerCode { get; set; } = null!;

    public bool? IsAccepted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime? ReceiptDate { get; set; }

    public string? PrefixNo { get; set; }

    public string? DocumentNo { get; set; }

    public string? PartyName { get; set; }

    public string? SourceType { get; set; }

    public string? PoType { get; set; }
}
