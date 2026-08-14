using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels
{
    public class WarrantyPackingSlip
    {
        public int Id { get; set; }
        public string DealerCode { get; set; } = null!;
        public int WarrantyInvoiceHeaderId { get; set; }
        public string? SlipPrefix { get; set; }
        public string SlipNo { get; set; } = null!;
        public DateTime SlipDate { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual WarrantyInvoice WarrantyInvoiceHeader { get; set; } = null!;
        public virtual ICollection<WarrantyPackingSlipBox> WarrantyPackingSlipBoxes { get; set; } = new List<WarrantyPackingSlipBox>();
    }

    public class WarrantyPackingSlipBox
    {
        public int Id { get; set; }
        public int WarrantyPackingSlipHeaderId { get; set; }
        public string BoxNumber { get; set; } = null!;
        public string? BoxType { get; set; }
        public string Length { get; set; } = "0";
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual WarrantyPackingSlip WarrantyPackingSlipHeader { get; set; } = null!;
        public virtual ICollection<WarrantyPackingSlipDetail> WarrantyPackingSlipDetails { get; set; } = new List<WarrantyPackingSlipDetail>();
    }

    public class WarrantyPackingSlipDetail
    {
        public int Id { get; set; }
        public int WarrantyPackingSlipBoxId { get; set; }
        public int WarrantyOrderGridDetailId { get; set; }
        public string? PrnNo { get; set; }
        public decimal Qty { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual WarrantyPackingSlipBox WarrantyPackingSlipBox { get; set; } = null!;
        public virtual WarrantyOrderGridDetail WarrantyOrderGridDetail { get; set; } = null!;
    }
}