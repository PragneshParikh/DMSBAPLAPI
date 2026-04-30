using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PdiOkVehicleChassisViewModel
    {
        public string ChassisNo { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemColor { get; set; }
        public int? MfgYear { get; set; }

        public string BatteryNo { get; set; }
        public string ConverterNo { get; set; }
        public string ChargerNo { get; set; }
        public string ControllerNo { get; set; }

        public string KeyNo { get; set; }
        public string BookNo { get; set; }

        public decimal? DealerPrice { get; set; }
        public decimal? CustomerPrice { get; set; }
        public string DealerCode { get; set; }

        public string BatteryChemical { get; set; }
        public string BatteryCapacity { get; set; }
        public string BatteryMake { get; set; }

        public string StockNo { get; set; }


        public decimal PreGstDisc { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal SGSTPer { get; set; }
        public decimal SGST { get; set; }
        public decimal CGSTPer { get; set; }
        public decimal CGST { get; set; }
        public decimal IGSTPer { get; set; }
        public decimal IGST { get; set; }
        public DateOnly? CustomerSaleDate { get; set; }
    }
}
