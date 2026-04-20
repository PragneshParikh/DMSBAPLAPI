using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ReceiptEntryViewModel
    {

        public string? Location { get; set; }

        public string ReceiptNo { get; set; } = null!;

        public string? SaleType { get; set; }

        public string? BookingId { get; set; }

        public string? PartyName { get; set; }

        public string? Financier { get; set; }

        public string ProductCode { get; set; } = null!;

        public string? SalesExecutive { get; set; }

        public string? ReceiptType { get; set; }
        public string? MobileNo { get; set; }
        public string? BillDate { get; set; }
        public string? BillNo { get; set; }
        public string? BusinessType { get; set; }


        public string? RefNo { get; set; }

        public string? Narration { get; set; }

        public decimal? TotalAmount { get; set; }

    }
}
