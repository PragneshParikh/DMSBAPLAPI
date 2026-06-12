using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ReceiptEntryDetail
{
    public int Id { get; set; }

    public int ReceiptId { get; set; }

    public int LineItemNo { get; set; }

    public string ReceiptType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ReceiptEntry Receipt { get; set; } = null!;
}
