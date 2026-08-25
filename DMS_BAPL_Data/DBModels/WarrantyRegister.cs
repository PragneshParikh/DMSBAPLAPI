using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels
{
    public class WarrantyRegister
    {
        public string SrNo { get; set; }
        public string ClaimType { get; set; }
        public string JobNo { get; set; }
        public DateTime? JobDate { get; set; }

        public string RbillNo { get; set; }
        public DateTime? RbillDate { get; set; }

        public string ItemName { get; set; }
        public string Description { get; set; }

        public string WarrantyClaimNo { get; set; }
        public DateTime? WarrantyClaimDate { get; set; }

        public string ChasisNo { get; set; }
        public string PartyName { get; set; }

        public string WarrantyClaimStatus { get; set; }
        public string ClaimAcceptRejectReason { get; set; }

        public string PrnNo { get; set; }

        public string WarrantyOrderStatus { get; set; }
        public string WarrantyOrderNo { get; set; }
        public DateTime? WarrantyOrderDate { get; set; }

        public string WarrantyInvoiceStatus { get; set; }
        public string WarrantyInvoiceNo { get; set; }
        public DateTime? WarrantyInvoiceDate { get; set; }

        public string PackingSlipNo { get; set; }
        public DateTime? PackingSlipDate { get; set; }

        public string DispatchNo { get; set; }
        public DateTime? DispatchDate { get; set; }

        public string DispatchReceivedStatus { get; set; }
        public DateTime? DispatchReceivedDate { get; set; }
        public string DispatchReceivedRemarks { get; set; }

        public DateTime? VerificationDate { get; set; }

        public string PackingConcern { get; set; }
        public string PackingConcernType { get; set; }
        public string PackingConcernRemarks { get; set; }

        public string MaterialConcern { get; set; }
        public string MaterialConcernType { get; set; }
        public string MaterialConcernRemarks { get; set; }
    }
}
