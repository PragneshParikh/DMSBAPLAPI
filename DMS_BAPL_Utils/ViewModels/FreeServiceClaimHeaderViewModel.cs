using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class FreeServiceClaimHeaderViewModel
    {
        public int Id { get; set; }

        public int srNo { get; set; }

        public string? ClaimPrefix { get; set; }

        public string? ClaimNo { get; set; }

        public DateTime? ClaimDate { get; set; }

        public string? DealerCode { get; set; }

        public string? LocationCode { get; set; }

        public bool? IsApproved { get; set; }

        public string? ActionBy { get; set; }

        public string? ActionDate { get; set; }

        public string? Remarks { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public string? UpdatedDate { get; set; }

        public int TotalCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
        public string? Status { get; set; }
    }
}
