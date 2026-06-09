using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ComplaintMasterViewModel
    {
        public int Id { get; set; }
        public string ComplaintName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
