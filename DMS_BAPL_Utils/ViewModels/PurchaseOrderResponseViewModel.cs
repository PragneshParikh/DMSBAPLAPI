using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PurchaseOrderResponseViewModel
    {
        public int Id { get; set; }
        public string PONumber { get; set; }
        public DateTime? PODate { get; set; }
        public string CustomerCode { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public bool? IsSubmitted { get; set; }
        public string TransactionType { get; set; }
        public string? Remarks { get; set; }
        public string? LocCode { get; set; }
        public string? LocationName { get; set; }
        public bool? IsAgainstKit { get; set; }
        public string? SubOrderType { get; set; }
        public string? LedgerCode { get; set; }
        public string? LedgerName { get; set; }
        public List<PurchaseOrderItemViewModel> Items { get; set; }
    }

    public class PurchaseOrderItemViewModel
    {
        public string ItemCode { get; set; }
        public int? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? Subsidy { get; set; }
        public decimal? MRP { get; set; }
        public string ItemDescription { get; set; }
        public int? LineNumber { get; set; }
        public List<TaxViewModel> Taxes { get; set; }
    }

    public class TaxViewModel
    {
        public string TaxCode { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
    }
}