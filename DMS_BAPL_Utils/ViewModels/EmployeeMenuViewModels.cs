using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class EmployeeMenuItemViewModel
    {
        public int SubMenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? PathName { get; set; }
    }

    public class EmployeeMenuGroupViewModel
    {
        public int TopMenuId { get; set; }
        public string TopMenuName { get; set; } = string.Empty;
        public List<EmployeeMenuItemViewModel> Items { get; set; } = new();
    }
}