using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class OemModelWarrantyViewModel
    {
        public int OemmodelId { get; set; }

        public DateOnly? EffectiveDate { get; set; }

        public decimal? Odoreading { get; set; }

        public string? DurationType { get; set; }

        public decimal? Duration { get; set; }

        public bool? IsB2b { get; set; }

    }
    public class OemModelWarrantyResponseViewModel
    {
        public int Id { get; set; } 

        public int OemmodelId { get; set; }
        public string Oemmodelname { get; set; }

        public DateOnly? EffectiveDate { get; set; }

        public decimal? Odoreading { get; set; }

        public string? DurationType { get; set; }

        public decimal? Duration { get; set; }

        public bool? IsB2b { get; set; }

    }
}
