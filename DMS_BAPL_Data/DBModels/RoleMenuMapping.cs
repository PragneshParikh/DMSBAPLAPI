using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class RoleMenuMapping
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Guid RoleId { get; set; }

    public string Category { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public int MenuId { get; set; }

    public string MenuName { get; set; } = null!;

    public string? PathName { get; set; }

    public string? ModuleName { get; set; }

    public bool CanView { get; set; }

    public bool CanAdd { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsActive { get; set; }
}
