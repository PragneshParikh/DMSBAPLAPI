using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class FreeServiceClaimViewModel
    {
        public int Id { get; set; }

        public string? ClaimPrefix { get; set; }

        public string? ClaimNo { get; set; }

        public DateTime? ClaimDate { get; set; }
        public string DealerCode { get; set; }
        public string? LocationCode { get; set; }



        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public string? UpdatedDate { get; set; }

        public List<FreeServiceClaimDetailViewModel> ItemDetails { get; set; }
    }

    public class FreeServiceClaimDetailViewModel
    {
        public int Id { get; set; }
        public int HeaderClaimId { get; set; }
        public int jobCardId { get; set; }
        public bool? IsApproved { get; set; }
        public string? ApprovedRejectBy { get; set; }
        public DateTime? ApprovedRejectDate { get; set; }
        public string? RejectReason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

}
