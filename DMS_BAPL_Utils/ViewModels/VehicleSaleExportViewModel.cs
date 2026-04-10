using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleExportViewModel
    {
        [JsonPropertyName("user")]
        public UserViewModel User { get; set; }

        [JsonPropertyName("vehicle")]
        public List<VehicleViewModel> Vehicle { get; set; }
    }
    public class UserViewModel
    {
        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("email_id")]
        public string? EmailId { get; set; }

        [JsonPropertyName("dateofbirth")]
        public string? DateOfBirth { get; set; }

        [JsonPropertyName("dateofanniversary")]
        public string? DateOfAnniversary { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("ModifiedOn")]
        public string? ModifiedOn { get; set; }

        [JsonPropertyName("IsDeleted")]
        public string? IsDeleted { get; set; }

        [JsonPropertyName("DeletedOn")]
        public string? DeletedOn { get; set; }

        [JsonPropertyName("Address1")]
        public string? Address1 { get; set; }

        [JsonPropertyName("Address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("State")]
        public string? State { get; set; }

        [JsonPropertyName("City")]
        public string? City { get; set; }
    }
    public class VehicleViewModel
    {
        [JsonPropertyName("chassis_no")]
        public string? ChassisNo { get; set; }

        [JsonPropertyName("model_id")]
        public string? ModelId { get; set; }

        [JsonPropertyName("motor_id")]
        public string? MotorId { get; set; }

        [JsonPropertyName("motor_controller_no")]
        public string? MotorControllerNo { get; set; }

        [JsonPropertyName("ecu_serial_no")]
        public string? EcuSerialNo { get; set; }

        [JsonPropertyName("ecu_imei_no")]
        public string? EcuImeiNo { get; set; }

        [JsonPropertyName("bike_sim_id")]
        public string? BikeSimId { get; set; }

        [JsonPropertyName("battery_serial_no")]
        public string? BatterySerialNo { get; set; }

        [JsonPropertyName("reg_number")]
        public string? RegNumber { get; set; }

        [JsonPropertyName("startdate")]
        public string? StartDate { get; set; }

        [JsonPropertyName("dealer_code")]
        public string? DealerCode { get; set; }

        [JsonPropertyName("salebill_no")]
        public string? SaleBillNo { get; set; }

        [JsonPropertyName("salebill_date")]
        public string? SaleBillDate { get; set; }

        [JsonPropertyName("FinancedBy")]
        public string? FinancedBy { get; set; }

        [JsonPropertyName("Item_Rate")]
        public string? ItemRate { get; set; }

        [JsonPropertyName("SGSTPer")]
        public string? SGSTPer { get; set; }

        [JsonPropertyName("SGSTAmnt")]
        public string? SGSTAmnt { get; set; }

        [JsonPropertyName("CGSTPer")]
        public string? CGSTPer { get; set; }

        [JsonPropertyName("CGSTAmnt")]
        public string? CGSTAmnt { get; set; }

        [JsonPropertyName("Net_Amnt")]
        public string? NetAmnt { get; set; }

        [JsonPropertyName("BatteryMake")]
        public string? BatteryMake { get; set; }

        [JsonPropertyName("ChargerNo")]
        public string? ChargerNo { get; set; }

        [JsonPropertyName("SaleType")]
        public string? SaleType { get; set; }
    }
}
