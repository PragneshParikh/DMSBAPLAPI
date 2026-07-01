using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class BgEmployeeListItemViewModel
    {
        public int Id { get; set; }
        public string? EmployeeCode { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? Zone { get; set; }
        public string? JobRoles { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? ReportingTo { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public string? ProfileImage { get; set; }
    }
}
