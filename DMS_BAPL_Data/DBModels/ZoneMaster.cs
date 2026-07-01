using System;

namespace DMS_BAPL_Data.DBModels
{
    public partial class ZoneMasters
    {
        public int Id { get; set; }
        public string Zone { get; set; } = string.Empty;
        public int? CityId { get; set; }
        public int? StateId { get; set; }
        public int? DealerId { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
