using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels
{
    public partial class WarrantyDispatchHeader
    {
        public int Id { get; set; }
        public string? DealerCode { get; set; }
        public string? SlipPrefix { get; set; }
        public int? DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public string? DispatchedThrough { get; set; }
        public string? ContactPerson { get; set; }
        public string? MobileNumber { get; set; }
        public decimal? CourierCharges { get; set; }
        public string? GrNo { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<WarrantyDispatchDetails> WarrantyDispatchDetails { get; set; } = new List<WarrantyDispatchDetails>();
    }
}
