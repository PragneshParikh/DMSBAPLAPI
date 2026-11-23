using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels
{
    [Table("DMS_VehicleWarranty")]
    public partial class DmsVehicleWarranty
    {
        public int Id { get; set; }

        public string? DealerName { get; set; }
        public string? DealerCode { get; set; }
        public string? ChasisNo { get; set; }
        public string? ItemCode { get; set; }
        public string? DisplayName { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public string? CustomerName { get; set; }
        public string? MobileNo { get; set; }
        public DateOnly? DateOfSales { get; set; }
        public string? DeviceGroup { get; set; }
        public string? DeviceType { get; set; }
        public int? VehicleStandardWarrantyMonths { get; set; }
        public int? VehicleStandardWarrantyOdo { get; set; }
        public string? VehicleExtWarrantyScheme { get; set; }
        public int? VehicleExtendedWarrantyMonths { get; set; }
        public decimal? VehicleExtendedWarrantyOdo { get; set; }
        public DateOnly? DateOfStandardWarrantyExpiry { get; set; }
        public DateOnly? DateOfExtendedWarrantyExpiry { get; set; }
        public DateOnly? LastOdoReadingDate { get; set; }
        public int? OdoReadingAsPerLastDate { get; set; }
        public string? CurrentWarrantyStatusAsPerDate { get; set; }
        public string? CurrentWarrantyStatusAsPerOdo { get; set; }
        public string? FinalWarrantyStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}