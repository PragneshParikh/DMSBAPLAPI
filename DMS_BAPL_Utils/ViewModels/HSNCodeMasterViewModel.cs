using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class HSNCodeMasterViewModel
    {
        public string Hsncode { get; set; } = null!;

        public string? Description { get; set; }

        public string Type { get; set; } = null!;
    }
}
