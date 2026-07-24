using System;

namespace DMS_BAPL_Data.DBModels;

public partial class BgRoleCategoryMapping
{
    public int Id { get; set; }
    public string? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? Category { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}