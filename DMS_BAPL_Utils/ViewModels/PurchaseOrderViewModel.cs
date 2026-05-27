using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PurchaseOrderViewModel
    {

        public string PONumber { get; set; }
        public DateTime PODate { get; set; }
        public string POType { get; set; }
        public string CustomerCode { get; set; }
        public string? TransactionType { get; set; }
        public string? Remarks { get; set; }
        public string? LocCode { get; set; }
        public string? LedgerCode { get; set; }
        public bool? IsAgainstKit { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? SubOrderType { get; set; }
        public List<PurchaseOrderDetailsViewModel> Items { get; set; }
    }

    public class PurchaseOrderDetailsViewModel
    {
        public string ItemCode { get; set; }
        public decimal Qty { get; set; }
        public decimal? MRP { get; set; }
    }

    public class UpdatePOStatusViewModel
    {
        public string PONumber { get; set; }
        public bool Status { get; set; }
        public string SaleOrderNo { get; set; }
        public string ConsigneeCode { get; set; }
    }
}

