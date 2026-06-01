using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class MaterialTransferExcelViewModel
    {
        public string? InvoiceNo { get; set; }
        public string? Chassisno { get; set; }
        public DateOnly? JobinDate { get; set; }
        public int? JobNo { get; set; }
        public string? Serviceloc { get; set; }
        public string? DealerCode { get; set; }
        public string? CustomerName { get; set; }
        public string? RegisterNo { get; set; }
    }
}
