using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class CircularDealerAssignmentViewModel
    {
        public int Id { get; set; }

        public int CircularId { get; set; }

        public string DealerCode { get; set; }

        public bool IsSelected { get; set; }

        public string? Status { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
