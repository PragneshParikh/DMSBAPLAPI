using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels
{
    public partial class WarrantyInvoice
    {
        public int Id { get; set; }
        public string DealerCode { get; set; } = null!;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? BatchDate { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? ClaimType { get; set; }
        public string? InvoicePrefix { get; set; }
        public int? SupplierId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string? ErpUniqueId { get; set; }
        public DateTime? ErpSubmittedDate { get; set; }

        public virtual ICollection<WarrantyInvoiceDetail> WarrantyInvoiceDetails { get; set; } = new List<WarrantyInvoiceDetail>();
    }

    public partial class WarrantyInvoiceDetail
    {
        public int Id { get; set; }
        public int WarrantyInvoiceHeaderId { get; set; }
        public int WarrantyOrderHeaderId { get; set; }
        public bool IsApproved { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual WarrantyInvoice WarrantyInvoiceHeader { get; set; } = null!;
        public virtual WarrantyOrder WarrantyOrderHeader { get; set; } = null!;
    }

    public partial class WarrantyInvoiceGridDetail
    {
        public int Id { get; set; }
        public int WarrantyInvoiceHeaderId { get; set; }
        public int WarrantyOrderHeaderId { get; set; }

        public string? OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? BatchDate { get; set; }

        public string? Location { get; set; }
        public string? LocationName { get; set; }
        public string? ClaimType { get; set; }

        public int? SupplierId { get; set; }
        public string? PartyName { get; set; }

        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalMrp { get; set; }

        public virtual WarrantyInvoice WarrantyInvoiceHeader { get; set; } = null!;
    }
}