using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class EmployeeDesignationWiseViewModel
    {
        public string EmployeeCode { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Designation { get; set; }

        public string? Department { get; set; }

        public string DealerCode { get; set; } = null!;
        public string? LocationCode { get; set; }

    }
}
