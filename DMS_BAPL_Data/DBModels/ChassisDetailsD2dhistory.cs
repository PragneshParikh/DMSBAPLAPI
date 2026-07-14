using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ChassisDetailsD2dhistory
{
    public int Id { get; set; }

    public int? LedgerId { get; set; }

    public string ChassisNo { get; set; } = null!;

    public string ItemCode { get; set; } = null!;

    public string? ItemName { get; set; }

    public string? ItemColor { get; set; }

    public string DealerCode { get; set; } = null!;

    public string LocationCode { get; set; } = null!;

    public string? IssueingDealerLocation { get; set; }

    public string? IssueingDealerCode { get; set; }

    public DateTime SaleDate { get; set; }

    public DateTime? TransDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
