using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleStockTransferCreateEditViewModel
    {
        public string TransferNo { get; set; }
        public DateTime TransferDate { get; set; }
        public string IssuingLocationCode { get; set; }
        public string IssuingStaffCode { get; set; }
        public string ReceivingLocationCode { get; set; }
        public string ReceivingStaffCode { get; set; }
        public string? Remarks { get; set; }
        public decimal TransferTotal { get; set; }
        public string DealerCode { get; set; }
        public List<VehicleStockTransferDetailViewModel> VehicleStockTransferDetailsViewModel { get; set; }
    }

    public class VehicleStockTransferDetailViewModel
    {
        public int? Id { get; set; }
        public string ChassisNo { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public decimal ItemRate { get; set; }
        public string? ModelName {  get; set; }
        public VehicleStockTransferChassisListViewModel? ChassisDetails {  get; set; }
    }

    public class VehicleStockTransferChassisListViewModel
    {
        public string ChassisNo { get; set; }
        public string ItemCode { get; set; }
        public string ModelName { get; set; }
        public string Colour { get; set; }
        public int? MfgYear { get; set; }
        public string KeyNo { get; set; }
        public string BatteryMake { get; set; }
        public string BatteryCapacity { get; set; }
        public string BatteryNo { get; set; }
        public string Charger { get; set; }
        public string Convertor { get; set; }
        public string Controller { get; set; }
        public decimal? FameII { get; set; }
        public decimal? Rate { get; set; }
    }

    public class VehicleStockTransferListVewModel
    {
        public int? Id { get; set; }
        public string TransferNo { get; set; }
        public DateTime TransferDate { get; set; }
        public string IssuingLocationCode { get; set; }
        public string IssuingLocationName { get; set; }
        public string IssuingStaffCode { get; set; }
        public string IssuingStaffName { get; set; }
        public string ReceivingLocationCode { get; set; }
        public string ReceivingLocationName { get; set; }
        public string ReceivingStaffCode { get; set; }
        public string ReceivingStaffName { get; set; }
        public string? Remarks { get; set; }
        public decimal TransferTotal { get; set; }
        public List<VehicleStockTransferDetailsWithChassisViewModel> VehicleStockTransferDetailsViewModel { get; set; }

    }
    public class VehicleStockTransferFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? IssuingLocation { get; set; }
        public string? ReceivingLocation { get; set; }
        public string? DealerCode { get; set; }
        public string? Search { get; set; }
    }
    public class VehicleStockTransferDetailsWithChassisViewModel
    {
        public int? Id { get; set; }
        public string ChassisNo { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public decimal ItemRate { get; set; }
        public string? ModelName { get; set; }
        public string Colour { get; set; }
        public int? MfgYear { get; set; }
        public string KeyNo { get; set; }
        public string BatteryMake { get; set; }
        public string BatteryCapacity { get; set; }
        public string BatteryNo { get; set; }
        public string Charger { get; set; }
        public string Convertor { get; set; }
        public string Controller { get; set; }
        public decimal? FameII { get; set; }
        public decimal? Rate { get; set; }
    }

    public class VehicleStockExcelViewModel
    {
        public int? id { get; set; }
        public string TransferNo { get; set; }
        public DateTime TransferDate { get; set; }
        public string? IssuingLocation { get; set; }
        public string? ReceivingLocation { get; set; }
        public string? IssuingStaff { get; set; }
        public string? ReceivingStaff { get; set; }
        public string? ChassisNo { get; set; }
        public string? ModelName { get; set; }
        public string? ItemCode { get; set; }
        public string? Colour { get; set; }
        public int? MfgYear { get; set; }
        public string? KeyNo { get; set; }
        public string? BatteryMake { get; set; }
        public string? BatteryNo { get;set; }
        public string? BatteryCapacity { get; set; }
        public string? Charger {  get; set; }
        public string? Controller { get; set; }

    }

}
