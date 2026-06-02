using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleExportViewModel
    {
        [JsonPropertyName("user")]
        public UserViewModel User { get; set; }

        // Sample JSON shows vehicle as an OBJECT, not an array
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

        [JsonPropertyName("ecu_bl_mac")]
        public string? EcuBlMac { get; set; }

        [JsonPropertyName("bike_sim_id")]
        public string? BikeSimId { get; set; }

        [JsonPropertyName("bike_mobile_no")]
        public string? BikeMobileNo { get; set; }

        [JsonPropertyName("sound_serial_no")]
        public string? SoundSerialNo { get; set; }

        [JsonPropertyName("sound_ble_mac")]
        public string? SoundBleMac { get; set; }

        [JsonPropertyName("battery_id")]
        public string? BatteryId { get; set; }

        [JsonPropertyName("battery_serial_no")]
        public string? BatterySerialNo { get; set; }

        [JsonPropertyName("reg_number")]
        public string? RegNumber { get; set; }

        [JsonPropertyName("validity")]
        public string? Validity { get; set; }

        [JsonPropertyName("startdate")]
        public string? StartDate { get; set; }

        [JsonPropertyName("dealer_code")]
        public string? DealerCode { get; set; }

        [JsonPropertyName("vendor_idno")]
        public string? VendorIdNo { get; set; }

        [JsonPropertyName("salebill_no")]
        public string? SaleBillNo { get; set; }

        [JsonPropertyName("salebill_date")]
        public string? SaleBillDate { get; set; }

        [JsonPropertyName("receiptguid")]
        public string? ReceiptGuid { get; set; }

        [JsonPropertyName("Item_Modl")]
        public string? ItemModel { get; set; }

        [JsonPropertyName("locationname")]
        public string? LocationName { get; set; }

        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("CustId")]
        public string? CustId { get; set; }

        [JsonPropertyName("DeletedOn")]
        public string? DeletedOn { get; set; }

        [JsonPropertyName("InvoiceDate")]
        public string? InvoiceDate { get; set; }

        [JsonPropertyName("InvoiceNo")]
        public string? InvoiceNo { get; set; }

        [JsonPropertyName("LocCode")]
        public string? LocCode { get; set; }

        [JsonPropertyName("LocationCity")]
        public string? LocationCity { get; set; }

        [JsonPropertyName("Pin")]
        public string? Pin { get; set; }

        [JsonPropertyName("AccountType")]
        public string? AccountType { get; set; }

        [JsonPropertyName("OEMModel")]
        public string? OEMModel { get; set; }

        [JsonPropertyName("Group1")]
        public string? Group1 { get; set; }

        [JsonPropertyName("HSNSACCode")]
        public string? HSNSACCode { get; set; }

        [JsonPropertyName("SaleType")]
        public string? SaleType { get; set; }

        [JsonPropertyName("FinancedBy")]
        public string? FinancedBy { get; set; }

        [JsonPropertyName("Item_Rate")]
        public decimal? ItemRate { get; set; }

        [JsonPropertyName("Insu_Amnt")]
        public decimal? InsuAmnt { get; set; }

        [JsonPropertyName("Regn_Amnt")]
        public decimal? RegnAmnt { get; set; }

        [JsonPropertyName("RegDiscAmnt")]
        public decimal? RegDiscAmnt { get; set; }

        [JsonPropertyName("DiscountType")]
        public decimal? DiscountType { get; set; }

        [JsonPropertyName("FameII")]
        public decimal? FameII { get; set; }

        [JsonPropertyName("StateFameII")]
        public decimal? StateFameII { get; set; }

        [JsonPropertyName("SGSTPer")]
        public decimal? SGSTPer { get; set; }

        [JsonPropertyName("SGSTAmnt")]
        public decimal? SGSTAmnt { get; set; }

        [JsonPropertyName("CGSTPer")]
        public decimal? CGSTPer { get; set; }

        [JsonPropertyName("CGSTAmnt")]
        public decimal? CGSTAmnt { get; set; }

        [JsonPropertyName("IGSTPer")]
        public decimal? IGSTPer { get; set; }

        [JsonPropertyName("IGSTAmnt")]
        public decimal? IGSTAmnt { get; set; }

        [JsonPropertyName("Net_Amnt")]
        public decimal? NetAmnt { get; set; }

        [JsonPropertyName("BatteryChemical")]
        public string? BatteryChemical { get; set; }

        [JsonPropertyName("BatteryCapacity")]
        public string? BatteryCapacity { get; set; }

        [JsonPropertyName("BatteryMake")]
        public string? BatteryMake { get; set; }

        [JsonPropertyName("ChargerNo")]
        public string? ChargerNo { get; set; }

        [JsonPropertyName("Converter")]
        public string? Converter { get; set; }

        [JsonPropertyName("VCU")]
        public string? VCU { get; set; }

        [JsonPropertyName("FameIIRequired")]
        public string? FameIIRequired { get; set; }

        [JsonPropertyName("SegmentName")]
        public string? SegmentName { get; set; }

        [JsonPropertyName("InstitutionalName")]
        public string? InstitutionalName { get; set; }

        [JsonPropertyName("SchemeName")]
        public string? SchemeName { get; set; }
    }
}