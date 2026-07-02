using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ZoneMasterViewModel
    {
        public int Id { get; set; }
        public string Zone { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
