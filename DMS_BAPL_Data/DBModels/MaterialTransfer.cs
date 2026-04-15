using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class MaterialTransfer
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int JobId { get; set; }

    public int? RackNo { get; set; }

    public int? Bin { get; set; }

    public int Technician { get; set; }

    public int? Ffi { get; set; }

    public int Quantity { get; set; }

    public decimal ItemRate { get; set; }

    public string? SerialNo { get; set; }

    public string? Remarks { get; set; }

    public string? ItemReceived { get; set; }

    public int? ValidDays { get; set; }

    public string IssueType { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public string? UpdatedDate { get; set; }

    public virtual ItemMaster Item { get; set; } = null!;
}
