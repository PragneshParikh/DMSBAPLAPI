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
        /// <summary>26.11.2026 - DealerMaster.IsActive (inactive dealers are flagged in the employee form)</summary>
        public bool IsActive { get; set; }
        /// <summary>26.11.2026 - "Zone Master" = dealer row in zone_master; "State" = dealer's state belongs to this zone in zone_master</summary>
        public string Source { get; set; } = "Zone Master";
    }

    /// <summary>26.11.2026 - dealers per zone (BG Employee form)</summary>
    public class ZoneDealerCountViewModel
    {
        public string Zone { get; set; } = string.Empty;
        /// <summary>dealers that have their own row in zone_master for this zone</summary>
        public int Mapped { get; set; }
        /// <summary>other dealers whose state belongs to this zone</summary>
        public int ByState { get; set; }
        public int Total { get; set; }
        public int Active { get; set; }
    }

    public class ZoneDealerCountResponse
    {
        public List<ZoneDealerCountViewModel> Zones { get; set; } = new();
        public int TotalDealers { get; set; }
        public int ActiveDealers { get; set; }
        /// <summary>dealers whose state is not in any zone (add them in Zone Master)</summary>
        public int NotInZone { get; set; }
        public int NotInZoneActive { get; set; }
        public List<string> NotInZoneStates { get; set; } = new();
    }

}
