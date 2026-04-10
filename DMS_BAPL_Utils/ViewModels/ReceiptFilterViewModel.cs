using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ReceiptFilterViewModel
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? ReceiptNo { get; set; }
        public string? PartyName { get; set; }
        public string? MobileNo { get; set; }
        public string? BookingId { get; set; }
        public string? Location { get; set; }
        public string? SaleType { get; set; }
        public string? ItemCode { get; set; }

    }
}
