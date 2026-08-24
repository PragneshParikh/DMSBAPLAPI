namespace DMS_BAPL_Utils.ViewModels
{
    public class BgRoleWithCategoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    public class UpdateBgRoleCategoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    public class LocationRoleDetailViewModel
    {
        public int LocationId { get; set; }
        public string? LocCode { get; set; }
        public string? LocName { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }

    public class UpdateLocationRoleDetailViewModel
    {
        public string LocCode { get; set; } = string.Empty;
        public string LocName { get; set; } = string.Empty;

        public string? RoleId { get; set; }
    }

    public class UpdateLocationMenuAccessViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string Module { get; set; }
        public string? Area { get; set; }
        public List<int> GrantedSubMenuIds { get; set; } = new();
    }
}