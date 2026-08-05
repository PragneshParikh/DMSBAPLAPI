using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ResolveRoleForItemsViewModel
    {
        public string Category { get; set; } = string.Empty;
        public List<int> SubMenuIds { get; set; } = new();
    }
}