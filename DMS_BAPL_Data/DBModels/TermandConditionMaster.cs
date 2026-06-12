using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class TermandConditionMaster
{
    public int Id { get; set; }

    public int? ConditionModule { get; set; }

    public string? TermCondition { get; set; }

    public DateTime? ConditionEffectiveDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
