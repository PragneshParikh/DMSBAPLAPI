using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ReceiptEntryEditViewModel
    {
        public int Id { get; set; }

        public string? Location { get; set; }

        public string ReceiptNo { get; set; } = null!;

        public string? MobileNo { get; set; }

        public DateOnly ReceiptDate { get; set; }

        public string? SaleType { get; set; }

        public string? BookingId { get; set; }

        public string? PartyName { get; set; }

        public string? Financier { get; set; }

        public string ProductCode { get; set; } = null!;
        public string? ProductName { get; set; }

        public string? ProductColor { get; set; }
        public string? ProductDescription { get; set; }

        public string? SalesExecutive { get; set; }

        public string? ReceiptType { get; set; }

        public string? RefNo { get; set; }

        public string? Narration { get; set; }
        public string? BusinessType {  get; set; }

        public decimal? TotalAmount { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
