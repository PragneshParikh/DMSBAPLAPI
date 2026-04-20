using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ParameterMasterTable
{
    public int Id { get; set; }

    public string ParameterName { get; set; } = null!;

    public decimal ParameterValue { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
