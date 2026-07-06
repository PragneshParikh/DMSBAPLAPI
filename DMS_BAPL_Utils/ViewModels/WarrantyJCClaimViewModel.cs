using DMS_BAPL_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class WarrantyJCClaimViewModel
    {
        public string? DealerCode { get; set; }
        public string? ClaimPrefix { get; set; }
        public int? ClaimNo { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string? ChassisNo { get; set; }
        public int? SupplierId { get; set; }
        public int? JobCardHeaderId { get; set; }
        public int? CustomerLedgerId { get; set; }
        public int? RepairBillHeaderId { get; set; }
        public int? FFIRId { get; set; }
        public string? ClaimAccount { get; set; }
        public string? DealerObservation { get; set; }

        public string? RootCauseAnalysis { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<IssueTypebasedJobDetails> repairBillDetails { get; set; }
    }
}
