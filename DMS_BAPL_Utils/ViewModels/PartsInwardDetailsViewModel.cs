using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PartsInwardDetailsViewModel
    {
        public string InvoiceNo { get; set; }
        public string selectedLocation { get; set; }
        public string POType { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string PrefixNo { get; set; }
        public string DocumentNo { get; set; }
        public string PartyCode { get; set; }
        public string SourceType { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
