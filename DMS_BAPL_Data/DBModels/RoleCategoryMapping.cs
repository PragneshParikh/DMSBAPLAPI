using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class RoleCategoryMapping
{
    public int Id { get; set; }

    public string Category { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }
}
