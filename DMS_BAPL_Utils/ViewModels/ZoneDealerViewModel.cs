using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ZoneDealerViewModel
    {
        public int ZoneMasterId { get; set; }
        public string Zone { get; set; }
        public int DealerId { get; set; }
        public string DealerName { get; set; }
        public string DealerCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
    }

}
