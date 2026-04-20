using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ReceiptEntry
{
    public int Id { get; set; }

    public string? Location { get; set; }

    public string ReceiptNo { get; set; } = null!;

    public string? MobileNo { get; set; }

    public DateOnly ReceiptDate { get; set; }

    public string? SaleType { get; set; }

    public string? BookingId { get; set; }

    public string? PartyName { get; set; }

    public string? Financier { get; set; }

    public string ProductCode { get; set; } = null!;

    public string? SalesExecutive { get; set; }

    public string? ReceiptType { get; set; }

    public string? RefNo { get; set; }

    public string? Narration { get; set; }

    public decimal? TotalAmount { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? BusinessType { get; set; }
}
