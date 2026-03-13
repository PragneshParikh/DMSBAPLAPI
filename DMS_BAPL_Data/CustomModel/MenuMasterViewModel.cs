using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.CustomModel
{
    public class MenuMasterViewModel
    {
        public int id { get; set; }
        public string label { get; set; }
        public string icon { get; set; }
        public bool isTitle { get; set; }
        public bool isCollapsed { get; set; }
        public string link { get; set; }
        public int? parentId { get; set; }
        public string module { get; set; }

        public List<MenuMasterViewModel> subItems { get; set; }
    }
}
