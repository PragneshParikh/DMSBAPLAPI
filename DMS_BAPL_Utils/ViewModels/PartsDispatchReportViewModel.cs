namespace DMS_BAPL_Utils.ViewModels
{
    public class PartsDispatchReportViewModel
    {
        public int SrNo { get; set; }

        public string? DealerName { get; set; }
        public string? DealerCode { get; set; }

        public string? CustomerName { get; set; }
        public string? MobileNo { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }

        public string? VehicleModel { get; set; }
        public string? VehicleVIN { get; set; }

        public string? BatteryMasterName { get; set; }

        public DateTime? DateOfSale { get; set; }

        public string? PartName { get; set; }
        public string? DeviceGroup { get; set; }
        public string? DeviceType { get; set; }
        public string? ItemDescription { get; set; }

        public decimal? VehicleStandardWarrantyMonths { get; set; }
        public decimal? VehicleStandardWarrantyODOReading { get; set; }

        public decimal? VehicleExtendedWarrantyMonths { get; set; }
        public decimal? VehicleExtendedWarrantyODOReading { get; set; }

        public DateTime? StandardWarrantyExpiryDate { get; set; }
        public DateTime? ExtendedWarrantyExpiryDate { get; set; }

        public DateTime? LastODOReadingDate { get; set; }
        public decimal? ODOReadingLastDate { get; set; }

        public string? CurrentWarrantyStatusDate { get; set; }
        public string? CurrentWarrantyStatusODO { get; set; }

        public string? FinalWarrantyStatus { get; set; }
    }
}