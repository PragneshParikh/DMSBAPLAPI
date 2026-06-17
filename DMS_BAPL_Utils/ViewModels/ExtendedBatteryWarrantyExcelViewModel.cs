using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ExtendedBatteryWarrantyExcelViewModel
    {
        public string SchemeName { get; set; }
        public string RateType { get; set; }
        public string DurationType { get; set; }
        public int Duration { get; set; }
        public decimal Kms { get; set; }
        public decimal DealerPrice { get; set; }
        public decimal CustomerPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public int Gstpercentage { get; set; }
        public int PurchaseValidity { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsActive { get; set; }
    }
}
