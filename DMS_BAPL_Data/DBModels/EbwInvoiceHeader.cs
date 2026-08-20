using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels
{
    public partial class EbwInvoiceHeader
    {
        public int Id { get; set; }
        public string DealerCode { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }
        public string? PrefixNo { get; set; }
        public int? BillNo { get; set; }

        public string LocationCode { get; set; } = null!;

        public string BillType { get; set; } = null!;
        public int? CashAccountId { get; set; }

        public int? SchemeId { get; set; }
        public string? SchemeName { get; set; }

        public string ChassisNo { get; set; } = null!;
        public string? SoldByDealerCode { get; set; }
        public DateTime? ChassisSaleDate { get; set; }
        public DateTime? ValidityExpiryDate { get; set; }

        public string? PartyName { get; set; }
        public string? PartyMobile { get; set; }
        public string? PartyAddress { get; set; }
        public string? PartyCity { get; set; }
        public string? PartyPincode { get; set; }
        public string? PartyState { get; set; }

        public string? DealerState { get; set; }
        public bool IsInterstate { get; set; }

        public string SerialNo { get; set; } = null!;
        public string? ItemCode { get; set; }

        public decimal PartsAmount { get; set; }
        public decimal NetAmount { get; set; }

        public string? Remarks { get; set; }
        public string Status { get; set; } = "Saved";
        public DateTime? WarrantyEndDate { get; set; }

        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual ICollection<EbwInvoiceDetail> EbwInvoiceDetails { get; set; } = new List<EbwInvoiceDetail>();
    }
}
