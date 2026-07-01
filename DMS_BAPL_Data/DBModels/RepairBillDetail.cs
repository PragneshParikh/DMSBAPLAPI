using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class RepairBillDetail
{
    public int Id { get; set; }

    public int? RepairBillId { get; set; }

    public int? MaterialId { get; set; }

    public int? LabourMasterId { get; set; }

    public int? PartWiseLabourId { get; set; }

    public int? PartItemId { get; set; }

    public string? ItemType { get; set; }

    public decimal? LabourQty { get; set; }

    public decimal? PartQty { get; set; }

    public decimal? LabourRate { get; set; }

    public decimal? PartRate { get; set; }

    public decimal? Fscrate { get; set; }

    public decimal? LabourDiscount { get; set; }

    public decimal? PartDiscount { get; set; }

    public string? DiscountType { get; set; }

    public decimal? Igstamount { get; set; }

    public decimal? Cgstamount { get; set; }

    public decimal? Sgstamount { get; set; }

    public int? IssutypeId { get; set; }

    public decimal? LabourTaxblAmount { get; set; }

    public decimal? PartTaxblAmount { get; set; }

    public decimal? LabourNetAmount { get; set; }

    public decimal? PartNetAmount { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual LabourMaster? LabourMaster { get; set; }

    public virtual MaterialTransfer? Material { get; set; }

    public virtual ItemMaster? PartItem { get; set; }

    public virtual PartWiseLabourMaster? PartWiseLabour { get; set; }

    public virtual RepairBillHeader? RepairBill { get; set; }

    public virtual ICollection<WarrantyJcclaimDetail> WarrantyJcclaimDetails { get; set; } = new List<WarrantyJcclaimDetail>();
}
