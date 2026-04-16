using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class CityResponseViewModel
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = null!;
        public int StateId { get; set; }
        public string StateName { get; set; } = null!;
        public bool? IsMetro { get; set; }
        public int? TierLevel { get; set; }
        public string? Abbreviation { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CityCreateEditViewModel
    {
        public string CityName { get; set; } = null!;
        public int StateId { get; set; }
         public bool? IsMetro { get; set; }
        public int? TierLevel { get; set; }
        public string? Abbreviation { get; set; }
        public bool? IsActive { get; set; }
    }
}
