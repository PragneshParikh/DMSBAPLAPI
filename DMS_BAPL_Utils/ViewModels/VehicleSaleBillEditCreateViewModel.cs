using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillEditCreateViewModel
    {
        public DateTime SaleDate { get; set; }
        public string SaleBillNo { get; set; } = null!;
        public bool IsD2d { get; set; }
        public string CustomerType { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string SaleType { get; set; } = null!;
        public string CashAccount { get; set; } 
        public string? Financier { get; set; } 
        public string BillType { get; set; } 
        public string BillFrom { get; set; } 
        public string CustomerName { get; set; } 
        public string BillingName { get; set; }
        public string SalesExecutive { get; set; } 
        public int? LedgerId { get; set; }
        public string TempRegNo { get; set; } 
        public string BookingId { get; set; } 
        public string PrintType { get; set; } 
        public string RefName { get; set; } 
        
       public string RefAddress { get; set; } 
        public string RefEmail { get; set; } 
        public int RefPoint { get; set; }
        public string RefRemarks { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public List<VehicleSaleBillDetailVM> Details { get; set; } = new();

    }

    public class VehicleSaleBillDetailVM
    {
        public string ChassisNo { get; set; } = null!;
        public decimal ItemRate { get; set; }
        public decimal PreGstDiscount { get; set; }
        public decimal RegAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public bool HasDevice { get; set; }
        public bool HasKit { get; set; }
        public bool IsDelivered { get; set; }
        public string Segment { get; set; } 
        public string InstitutionalType { get; set; } 
        public string SchemeName { get; set; } 
        public string Narration { get; set; }
        public decimal? Sgstper { get; set; }
        public decimal? Sgstamnt { get; set; }
        public decimal? Cgstper { get; set; }
        public decimal? Cgstamnt { get; set; }
        public decimal? Igstper { get; set; }
        public decimal? Igstamnt { get; set; }
        public int? MfgYear { get; set; }

        public string? InsNo { get; set; }

        public DateOnly? InsStartDate { get; set; }


        public string? RegNo { get; set; }

        public DateOnly? InsExpDate { get; set; }
        public decimal FinalAmount { get; set; }
        public bool IsAgainstExchange { get; set; }
        public string? ModelName { get; set; }

        public string? Colour { get; set; }

        public string? Battery { get; set; }

        public string? ConvertorNo { get; set; }

        public string? ChargerNo { get; set; }

        public string? ControllerNo { get; set; }

        public string? Key { get; set; }

        public string? BookNo { get; set; }

        public string? ExtWarranty { get; set; }

        public string? BatteryChemical { get; set; }

        public string? BatteryCapacity { get; set; }

        public string? BatteryMake { get; set; }

        public string? StockDetailsNo { get; set; }

        public string? Vcu { get; set; }

    }
}
