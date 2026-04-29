using System;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PurchaseOrderSearchViewModel
    {
        public string? PurchaseNo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? TransactionType { get; set; }
        public string? IsSubmitted { get; set; }
    }
}
