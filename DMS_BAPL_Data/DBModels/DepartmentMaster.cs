using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class DepartmentMaster
{
    public int DepartmentId { get; set; }
    public string? Abbreviation { get; set; }          // was: string ... = null!
    public string DepartmentName { get; set; } = null!;
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public virtual ICollection<DesignationMaster> DesignationMasters { get; set; } = new List<DesignationMaster>();
}