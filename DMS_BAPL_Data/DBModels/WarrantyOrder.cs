using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels
{
    public partial class WarrantyOrder
    {
        public int Id { get; set; }
        public string? DealerCode { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string BatchNo { get; set; } = null!;
        public DateTime BatchDate { get; set; }

        public string OrderNo { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string Location { get; set; } = null!;

        public string ClaimType { get; set; } = null!;
        public int SupplierId { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;

        public virtual ICollection<WarrantyOrderDetail> WarrantyOrderDetails { get; set; } = new List<WarrantyOrderDetail>();
    }

    public partial class WarrantyOrderDetail
    {
        public int Id { get; set; }
        public int WarrantyOrderHeaderId { get; set; }
        public int WarrantyJcclaimId { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsApproved { get; set; } = false;

        public virtual WarrantyOrder WarrantyOrderHeader { get; set; } = null!;
        public virtual WarrantyJcclaim WarrantyJcclaim { get; set; } = null!;
    }
}