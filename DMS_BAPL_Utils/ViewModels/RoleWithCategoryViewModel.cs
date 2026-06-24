using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class RoleWithCategoryViewModel
    {
        public string Name { get; set; } = string.Empty;      // role name
        public string Category { get; set; } = string.Empty;  // 'Sales' / 'Service'
    }
}
