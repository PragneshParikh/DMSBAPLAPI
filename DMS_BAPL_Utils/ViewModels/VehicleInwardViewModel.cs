using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleInwardViewModel
    {
        public DateOnly? invoice_date { get; set; }

        public string? invoice_no { get; set; }

        public int? mfg_year { get; set; }

        public string? item_code { get; set; }

        public string? colr_code { get; set; }

        public string? chasis_no { get; set; }

        public string? motor_no { get; set; }

        public string? key_no { get; set; }

        public string? serv_bkno { get; set; }

        public string? battery_id { get; set; }

        public string? battery_no { get; set; }

        public string? battery_no2 { get; set; }

        public string? battery_no3 { get; set; }

        public string? battery_no4 { get; set; }

        public string? battery_no5 { get; set; }

        public string? battery_no6 { get; set; }

        public string? ecu_serno { get; set; }

        public string? ecu_im_ei { get; set; }

        public string? ecu_bal_mac { get; set; }

        public string? immoblizer_status { get; set; }

        public string? immoblizer_no { get; set; }

        public string? bike_simid { get; set; }

        public string? bike_mobileno { get; set; }

        public string? charger_no { get; set; }

        public string? controller_no { get; set; }

        public string? soundbar_serno { get; set; }

        public string? soundbar_bal_mac { get; set; }

        public string? voltage { get; set; }

        public string? regnumber { get; set; }

        public string? validity { get; set; }

        public string? startdate { get; set; }

        public string? tyre_no1 { get; set; }

        public string? tyre_no2 { get; set; }

        public int? gst_idno { get; set; }

        public string? loc_code { get; set; }

        public string? dealer_code { get; set; }

        public string? battery_chemistry { get; set; }

        public string? battery_capacity { get; set; }

        public string? battery_make { get; set; }

        public int? battery_idno { get; set; }

        public decimal? fame2_discount { get; set; }

        public string? converter { get; set; }

        public string? vcu { get; set; }

        public string? ordertype { get; set; }

        public int? mfg_month { get; set; }

        public decimal? dlrprice { get; set; }

        public decimal? custprice { get; set; }

        public string? poType { get; set; }

        public bool? IsAccepted { get; set; }
        public string DMSPoNo { get; set; }
    }
    public class VehicleInwardD2DViewModel
    {
        public int Id { get; set; }

        public DateOnly? InvoiceDate { get; set; }

        public string? InvoiceNo { get; set; }

        public int? MfgYear { get; set; }

        public string? ItemCode { get; set; }

        public string? ColrCode { get; set; }

        public string? ChasisNo { get; set; }

        public string? MotorNo { get; set; }

        public string? KeyNo { get; set; }

        public string? ServBkno { get; set; }

        public string? BatteryId { get; set; }

        public string? BatteryNo { get; set; }

        public string? BatteryNo2 { get; set; }

        public string? BatteryNo3 { get; set; }

        public string? BatteryNo4 { get; set; }

        public string? BatteryNo5 { get; set; }

        public string? BatteryNo6 { get; set; }

        public string? EcuSerno { get; set; }

        public string? EcuImEi { get; set; }

        public string? EcuBalMac { get; set; }

        public string? ImmoblizerStatus { get; set; }

        public string? ImmoblizerNo { get; set; }

        public string? BikeSimid { get; set; }

        public string? BikeMobileno { get; set; }

        public string? ChargerNo { get; set; }

        public string? ControllerNo { get; set; }

        public string? SoundbarSerno { get; set; }

        public string? SoundbarBalMac { get; set; }

        public string? Voltage { get; set; }

        public string? Regnumber { get; set; }

        public string? Validity { get; set; }

        public string? Startdate { get; set; }

        public string? TyreNo1 { get; set; }

        public string? TyreNo2 { get; set; }

        public int? GstIdno { get; set; }

        public string? LocCode { get; set; }

        public string? DealerCode { get; set; }

        public string? BatteryChemistry { get; set; }

        public string? BatteryCapacity { get; set; }

        public string? BatteryMake { get; set; }

        public int? BatteryIdno { get; set; }

        public decimal? Fame2Discount { get; set; }

        public string? Converter { get; set; }

        public string? Vcu { get; set; }

        public string? Ordertype { get; set; }

        public int? MfgMonth { get; set; }

        public bool? IsAccepted { get; set; }

        public decimal? Dlrprice { get; set; }

        public decimal? Custprice { get; set; }

        public string? PoType { get; set; }

        public string? Ponumber { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool? IsD2d { get; set; }

        public string? InwardType { get; set; }
        public string? IssuedDealerName { get; set; }
        public string? IssuedDealerCode { get; set; }
    }
}
