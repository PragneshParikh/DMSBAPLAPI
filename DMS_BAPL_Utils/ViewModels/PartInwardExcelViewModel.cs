using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PartInwardExcelViewModel
    {
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string DealerCode { get; set; }
        public string PartyName { get; set; }
        public string LocationCode { get; set; }
        public int TotalItems { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsAccepted { get; set; }
        public int GST { get; set; }
    }
}
