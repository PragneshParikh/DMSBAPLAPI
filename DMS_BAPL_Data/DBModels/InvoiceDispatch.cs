using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels
{

    public class InvoiceDispatch
    {
        public int Id { get; set; }

        public string InvoiceNo { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }

        public string DispatchType { get; set; } = null!; // "PART" or "VEHICLE"

        public string? DealerCode { get; set; }
        public string? LocCode { get; set; }

        // ---- Part-specific fields ----
        public string? PartNo { get; set; }
        public string? ItemHsncode { get; set; }
        public int? ItemQty { get; set; }
        public decimal? ItemRate { get; set; }
        public decimal? ItemMrp { get; set; }
        public decimal? ItemDisc { get; set; }
        public string? DiscountType { get; set; }
        public decimal? Cgst { get; set; }
        public decimal? Sgst { get; set; }
        public decimal? Igst { get; set; }

        // ---- Vehicle-specific fields ----
        public string? ChasisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? ItemCode { get; set; }
        public string? ColrCode { get; set; }
        public string? MfgYear { get; set; }
        public string? MfgMonth { get; set; }
        public decimal? Dlrprice { get; set; }
        public decimal? Custprice { get; set; }

        // ---- Shared status / audit ----
        public bool IsAccepted { get; set; }

        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
