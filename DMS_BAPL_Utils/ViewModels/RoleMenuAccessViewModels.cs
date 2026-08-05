using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class UpdateRoleMenuAccessViewModel
    {
        public List<int> GrantedSubMenuIds { get; set; } = new();
    }

    public class RoleMenuAccessResponseViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string? RoleName { get; set; }
        public string? Category { get; set; }
        public List<DealerMenuAccessGroupViewModel> Groups { get; set; } = new();
    }

    public class UpdateRoleCategoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}