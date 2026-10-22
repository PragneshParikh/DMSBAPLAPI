using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels
{
    public partial class WarrantyDispatchDetails
    {
        public int Id { get; set; }
        public int WarrantyDispatchHeaderId { get; set; }
        public int WarrantyPackingSlipHeaderId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual WarrantyDispatchHeader WarrantyDispatchHeader { get; set; } = null!;
    }
}
