using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class CounterBillHeaderViewModel
    {
        public int? Id { get; set; }
        public string? DealerCode { get; set; }
        public string BillNo { get; set; } = null!;

        public DateTime BillDate { get; set; }

        public string BillType { get; set; } = null!;

        public string LocCode { get; set; } = null!;
        public string? LocName { get; set; } = null!;
        public string? DealerName { get; set; } = null!;

        public string? CashCreditAcc { get; set; }

        public string? PartyName { get; set; }
        public string? MobileNo { get; set; }
        public string? LocationName { get; set; }

        public int? PartyState { get; set; }

        public string? ChassisNo { get; set; }

        public decimal BillAmount { get; set; }

        public string? Remarks { get; set; }

        public int? CustomerLedgerId { get; set; }
    }
    public class CounterBillDetailsViewModel
    {
        public int CounterBillId { get; set; }

        public string PartCode { get; set; } = null!;
        public string? PartName { get; set; }
        public string? HSNCode { get; set; }

        public string? SaleType { get; set; }

        public int Qty { get; set; }

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
    public class CounterBillViewModel
    {

        public CounterBillHeaderViewModel Header { get; set; } = new();

        public List<CounterBillDetailsViewModel> Details { get; set; } = new();
    }
}
