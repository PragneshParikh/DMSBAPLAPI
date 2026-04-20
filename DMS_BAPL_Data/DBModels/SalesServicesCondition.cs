using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class SalesServicesCondition
{
    public int Id { get; set; }

    public string? ConditionType { get; set; }

    public int? SrNo { get; set; }

    public string? ConditionText { get; set; }

    public string? Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
