using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class WarrantyDispatchViewModel
    {
        public string? DealerCode { get; set; }
        public string? SlipPrefix { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string? DispatchedThrough { get; set; }
        public string? ContactPerson { get; set; }
        public string? MobileNumber { get; set; }
        public decimal? CourierCharges { get; set; }
        public string? GrNo { get; set; }
        public List<int> PackingSlipIds { get; set; } = new List<int>();
    }
}
