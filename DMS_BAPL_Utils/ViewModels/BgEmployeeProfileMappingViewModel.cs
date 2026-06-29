using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class BgEmployeeProfileMappingViewModel
    {
        public int Id { get; set; }
        public int BgEmployeeId { get; set; }
        public int EmployeeId { get; set; }
        public int ProfileId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Read-only display fields (populated on GET)
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? ProfileName { get; set; }
    }

    // =========================================================
    // Bulk save request — replaces all mappings for a BG employee
    // =========================================================

    public class BgEmployeeProfileMappingSaveRequest
    {
        public int BgEmployeeId { get; set; }
        public List<BgEmployeeProfileMappingViewModel> Mappings { get; set; } = new();
        public string? CreatedBy { get; set; }
    }
}
