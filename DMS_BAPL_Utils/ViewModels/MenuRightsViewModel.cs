using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    // A parent menu group with its clickable sub-menus, each carrying the
    // dealer's currently saved permission bitmask (0 if never granted).
    public class MenuGroupViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = null!;
        public string? ModuleName { get; set; }
        public List<SubMenuRightViewModel> SubMenus { get; set; } = new();
    }

    public class SubMenuRightViewModel
    {
        public int SubMenuId { get; set; }
        public string MenuName { get; set; } = null!;
        public string? PathName { get; set; }
        public int Permission { get; set; }   // bitmask — 0 if not granted
    }

    

    public class SaveMenuRightsRequest
    {
        public string DealerCode { get; set; } = null!;
        public List<MenuRightItem> Rights { get; set; } = new();
    }

    public class MenuRightItem
    {
        public int MenuId { get; set; }
        public int SubMenuId { get; set; }
        public int Permission { get; set; }   // bitmask
    }
}