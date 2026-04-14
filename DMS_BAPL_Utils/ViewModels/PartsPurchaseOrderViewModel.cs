using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PartsPurchaseOrderViewModel
    {
        public string PONumber { get; set; }
        public DateTime PODate { get; set; }
        public string POType { get; set; }
        public string CustomerCode { get; set; }
        public string? TransactionType { get; set; }
        public List<PartsPurchaseOrderDetailsViewModel> Items { get; set; }
    }

    public class PartsPurchaseOrderDetailsViewModel
    {
        public string ItemCode { get; set; }
        public decimal Qty { get; set; }
        // Note: MRP is handled on the UI side for now, not stored in DB.
    }
}
