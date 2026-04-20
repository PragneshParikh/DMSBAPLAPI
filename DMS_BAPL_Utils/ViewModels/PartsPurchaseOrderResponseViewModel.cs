using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PartsPurchaseOrderResponseViewModel
    {
        public string PONumber { get; set; }
        public DateTime? PODate { get; set; }
        public string CustomerCode { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public bool? IsSubmitted { get; set; }
        public string TransactionType { get; set; }

        public List<PartsPurchaseOrderItemViewModel> Items { get; set; }
    }

    public class PartsPurchaseOrderItemViewModel
    {
        public string ItemCode { get; set; }
        public int? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? Subsidy { get; set; }

        public List<PartsTaxViewModel> Taxes { get; set; }
    }

    public class PartsTaxViewModel
    {
        public string TaxCode { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
