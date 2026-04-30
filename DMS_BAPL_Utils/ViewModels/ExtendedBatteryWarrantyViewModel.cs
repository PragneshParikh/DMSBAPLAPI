using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ExtendedBatteryWarrantyViewModel
    {
        public int Id { get; set; }

        public int OemmodelId { get; set; }

        public string SchemeName { get; set; } = null!;

        public int RateType { get; set; }

        public int Duration { get; set; }

        public int DurationType { get; set; }

        public decimal Kms { get; set; }

        public decimal DealerPrice { get; set; }

        public decimal CustomerPrice { get; set; }

        public decimal DiscountAmount { get; set; }

        public int Gstpercentage { get; set; }

        public int PurchaseValidity { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

    }
}
