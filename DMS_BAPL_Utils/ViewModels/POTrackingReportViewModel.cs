using System;

namespace DMS_BAPL_Utils.ViewModels
{
    public class POTrackingReportViewModel
    {
        public int SrNo { get; set; }

        public string? DealerName { get; set; }

        public string? DealerCode { get; set; }

        public string? LocationName { get; set; }

        public string? OrderNumber { get; set; }

        public DateTime? OrderDate { get; set; }

        public DateTime? SubmitToERPDate { get; set; }

        public string? POType { get; set; }

        public decimal POQty { get; set; }

        public decimal BilledQty { get; set; }

        public decimal PendingQty { get; set; }

        public decimal Archived { get; set; }

        public decimal POPrice { get; set; }

        public decimal BilledPrice { get; set; }

        public decimal PendingPOPrice { get; set; }

        public decimal ArchivedPriceExclGST { get; set; }

        public string? POStatus { get; set; }

        public string? UniqueId { get; set; }

        public string? DealerPONo { get; set; }

        public decimal WalletDebit { get; set; }

        public decimal PGDebit { get; set; }

        public string? PGStatus { get; set; }

        public string? PaymentLink { get; set; }

        public string? PaymentType { get; set; }

        public string? TempPONo { get; set; }

        public string? MerchantOrderNo { get; set; }

        public string? MerchantOrderStatus { get; set; }
    }
}