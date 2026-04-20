using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class RoleWiseMenuRight
{
    public int Id { get; set; }

    public string RoleId { get; set; } = null!;

    public int MenuId { get; set; }

    public int SubMenuId { get; set; }

    public int Permission { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual AspNetRole Role { get; set; } = null!;

    public virtual MenuMaster SubMenu { get; set; } = null!;
}
