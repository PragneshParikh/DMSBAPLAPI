using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.ViewModels
{
    public class ColorMasterViewModel
    {
        public int rrgcoloridno { get; set; }
        public string? colorname { get; set; } = null!;
        public string? colorcode { get; set; } = null!;
        public int createdby { get; set; }
        public DateTime createddatetime { get; set; }
        public int? updatedby { get; set; }
        public DateTime? updateddatetime { get; set; }
    }
}
