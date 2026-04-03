using DMS_BAPL_Data.DBModels;
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
        public string CashAccount { get; set; } = null!;
        public string Financier { get; set; } = null!;
        public string BillType { get; set; } = null!;
        public string BillFrom { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string BillingName { get; set; } = null!;
        public string SalesExecutive { get; set; } = null!;
        public string TempRegNo { get; set; } = null!;
        public string BookingId { get; set; } = null!;
        public string PrintType { get; set; } = null!;
        public string RefName { get; set; } = null!;
        public string RefAddress { get; set; } = null!;
        public string RefEmail { get; set; } = null!;
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
        public string Segment { get; set; } = null!;
        public string InstitutionalType { get; set; } = null!;
        public string SchemeName { get; set; } = null!;
        public string Narration { get; set; } = null!;
        public decimal FinalAmount { get; set; }
        public bool IsAgainstExchange { get; set; }

    }
}
