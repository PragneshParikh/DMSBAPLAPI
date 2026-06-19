using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class DesignationMaster
{
    public int DesignationId { get; set; }

    public string? Abbreviation { get; set; }

    public string DesignationName { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual DepartmentMaster? Department { get; set; }
}
