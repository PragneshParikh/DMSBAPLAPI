namespace DMS_BAPL_Utils.ViewModels
{
    public class DealerMenuAccessItemViewModel
    {
        public int SubMenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? PathName { get; set; }
        public bool IsGranted { get; set; }
    }

    public class DealerMenuAccessGroupViewModel
    {
        public int TopMenuId { get; set; }
        public string TopMenuName { get; set; } = string.Empty;
        public List<DealerMenuAccessItemViewModel> Items { get; set; } = new();
    }

    public class DealerMenuAccessResponseViewModel
    {
        public int DealerId { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<DealerMenuAccessGroupViewModel> Groups { get; set; } = new();
    }

    public class UpdateDealerMenuAccessViewModel
    {
        public List<int> GrantedSubMenuIds { get; set; } = new();

        public string RoleId { get; set; } = string.Empty;
    }

    public class DealerLocationViewModel
    {
        public int Id { get; set; }
        public string? LocCode { get; set; }
        public string? LocName { get; set; }
        public bool IsActive { get; set; }
    }

    public class BulkUpdateLocationStatusViewModel
    {
        public List<int> LocationIds { get; set; } = new();
        public bool IsActive { get; set; }
    }
}