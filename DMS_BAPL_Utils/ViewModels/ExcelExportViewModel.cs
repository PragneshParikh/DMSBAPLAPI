using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ExcelExportViewModel
    {
        public string SheetName { get; set; }
        public List<string> Columns { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
    }
}

