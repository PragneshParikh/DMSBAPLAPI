using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class EbwInvoiceDetailViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string HsnCode { get; set; }
        public int Qty { get; set; }
        public decimal ItemMrp { get; set; }
        public decimal BaseItemRate { get; set; }
        public decimal ItemRate { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal IgstPer { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal CgstPer { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPer { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal Amount { get; set; }
    }
}
