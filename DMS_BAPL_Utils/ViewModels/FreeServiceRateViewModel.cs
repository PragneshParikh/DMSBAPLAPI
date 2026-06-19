using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class FreeServiceRateViewModel
    {
        public int Id { get; set; }
        public int OemmodelId { get; set; }
        public string? OEMModelName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int? ServiceId { get; set; }
        public decimal? MetroRate { get; set; }
        public decimal? MetroGst { get; set; }
        public decimal? NonMetroRate { get; set; }
        public decimal? NonMetroGst { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
