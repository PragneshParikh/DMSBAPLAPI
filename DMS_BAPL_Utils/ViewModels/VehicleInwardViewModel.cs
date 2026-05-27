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
    }
}
