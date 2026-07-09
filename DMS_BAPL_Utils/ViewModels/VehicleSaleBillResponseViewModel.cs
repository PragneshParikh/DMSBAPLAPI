using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillResponseViewModel
    {
        public int Id { get; set; }
        public string SaleBillNo { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public int? LedgerId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Location { get; set; }
        public DateTime SaleDate { get; set; }
        public string SaleType { get; set; }
        public int? BillType { get; set; }
        public string BillFrom { get; set; }
        public string BillingName { get; set; }
        public int? Financier { get; set; }

        public string? FinancierName { get; set; }
        public string? CashAccount { get; set; }
        public string? SalesExecutive { get; set; }
        public string? isTempRegNo { get; set; }
        public string? TempRegNo { get; set; }
        public bool? isD2d { get; set; }
        public string? CustomerType { get; set; }
        public string? Status { get; set; }
        public string DealerCode { get; set; }
        public string BookingId { get; set; }
        public string PrintType { get; set; }
        public string? RefName { get; set; }
        public string? RefAddress { get; set; }
        public string? RefEmail { get; set; }
        public string? RefMobile { get; set; }
        public int? RefPoint { get; set; }
        public string? RefRemarks { get; set; }
        public string? AccessoryBillNo { get; set; }

        public decimal? AccessoryAmount { get; set; }

        public decimal? NoPlateAmount { get; set; }

        public decimal? Hpamount { get; set; }

        public decimal? HandlingCharges { get; set; }

        public decimal? StateSubsidyAmount { get; set; }
        public string? hsnCode { get; set; }
        public string? motorNo { get; set; }
        public List<SalesConditionViewModel>? TermsAndConditions { get; set; }



        public List<VehicleSaleBillDetailVM> Details { get; set; } = new();



    }

    public class SalesConditionViewModel
    {
        public int Id { get; set; }
        public int? SrNo { get; set; }
        public string? ConditionText { get; set; }

    }
}
