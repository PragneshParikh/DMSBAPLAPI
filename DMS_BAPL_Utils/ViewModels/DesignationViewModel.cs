using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class DesignationViewModel
    {
        public int DesignationId { get; set; }

        public string DesignationCode { get; set; } = null!;

        public string DesignationName { get; set; } = null!;

        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

    }
}
