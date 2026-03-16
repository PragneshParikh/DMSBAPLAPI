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

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<RoleWiseMenuRight> RoleWiseMenuRights { get; set; } = new List<RoleWiseMenuRight>();
}
