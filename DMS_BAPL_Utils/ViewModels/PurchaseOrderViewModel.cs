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
        public bool? IsAgainstKit { get; set; }
        public List<PurchaseOrderDetailsViewModel> Items { get; set; }
    }

    public class PurchaseOrderDetailsViewModel
    {
        public string ItemCode { get; set; }
        public decimal Qty { get; set; }
        public decimal? MRP { get; set; }
    }
}

