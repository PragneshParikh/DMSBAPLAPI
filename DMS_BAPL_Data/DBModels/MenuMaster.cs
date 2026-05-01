using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class MenuMaster
{
    public int Id { get; set; }

    public string MenuName { get; set; } = null!;

    public int? ParentMenuId { get; set; }

    public string? PathName { get; set; }

    public int? SerialNo { get; set; }

    public string? ModuleName { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<RoleWiseMenuRight> RoleWiseMenuRights { get; set; } = new List<RoleWiseMenuRight>();
}
