using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class CounterBillExcelViewModel
    {
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string BillNo { get; set; } = null!;

        public DateTime BillDate { get; set; }

        public string BillType { get; set; } = null!;

        public string LocCode { get; set; } = null!;

        public string? CashCreditAcc { get; set; }

        public string? PartyName { get; set; }
        public string? MobileNo { get; set; }
        public string? LocationName { get; set; }

        public string? PartyState { get; set; }

        public string? ChassisNo { get; set; }

        public decimal BillAmount { get; set; }

        public string? Remarks { get; set; }

        public int? CustomerLedgerId { get; set; }
        public string PartCode { get; set; } = null!;
        public string? PartDescription { get; set; }
        public string? Address { get; set; }

        public string? SaleType { get; set; }

        public decimal Qty { get; set; }

        public decimal Rate { get; set; }

        public string? DiscType { get; set; }

        public decimal Discount { get; set; }

        public decimal Mrp { get; set; }

        public decimal Igstper { get; set; }

        public decimal Igstamnt { get; set; }

        public decimal Cgstper { get; set; }

        public decimal Cgstamnt { get; set; }

        public decimal Sgstper { get; set; }

        public decimal Sgstamnt { get; set; }
    }

    public class CounterBillPrintViewModel
    {
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerAddress1 { get; set; }
        public string? DealerAddress2 { get; set; }
        public string? Pin { get; set; }
        public string? ChassisNo { get; set; }
        public string? BillType { get; set; }
        public string? PhoneNo1{ get; set; }
        public string? PhoneNo2{ get; set; }
        public string? PanNo{ get; set; }
        public string? GSTNo{ get; set; }
        public string? InvoiceNo{ get; set; }
        public DateTime? InvoiceDate{ get; set; }
        public string? CustomerName{ get; set; }
        public string? CustomerMobile{ get; set; }
        public string? CustomerAddress{ get; set; }
        public string? State{ get; set; }
        public string? City{ get; set; }
        public string? Remarks{ get; set; }
        public string? CustomerGST{ get; set; }
        public List<SalesConditionViewModel>? TermsAndConditions{ get; set; }
        public List<CounterBillDetailsViewModel> Details { get; set; }

    }
}
