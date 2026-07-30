using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class EmployeeRoleMapping
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Category { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string? RoleId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
