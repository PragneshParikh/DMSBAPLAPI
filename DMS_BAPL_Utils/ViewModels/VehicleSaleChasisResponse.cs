using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleChasisResponse
    {
        public string ChassisNo { get; set; }
        public string ItemCode { get; set; }

        //public decimal ItemRate { get; set; }
        public decimal DealerRate { get; set; }
        public decimal CustomerRate { get; set; }
        public decimal PreGstDis { get; set; }

        public decimal CGSTPer { get; set; }
        public decimal CGSTAmt { get; set; }

        public decimal SGSTPer { get; set; }
        public decimal SGSTAmt { get; set; }

        public decimal IGSTPer { get; set; }
        public decimal IGSTAmt { get; set; }

        public int? MfgYear { get; set; }
    }

    public class VehicleSaleChasisRequest
    {
        public string DealerCode { get; set; }
        public int LedgerId { get; set; }
        //public bool IsD2d { get; set; }
        //public string ItemCode { get; set; }
    }
}