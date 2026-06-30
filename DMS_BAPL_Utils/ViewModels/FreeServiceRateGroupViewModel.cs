using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class FreeServiceRateGroupViewModel
    {
        public int SrNo { get; set; }
        public int? OEMModelId { get; set; }
        public string OEMModelName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<FreeServiceRateViewModel> Services { get; set; }
    }
}
