using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class WarrantyJcclaim
{
    public int Id { get; set; }

    public string? DealerCode { get; set; }

    public string? ClaimPrefix { get; set; }

    public int? ClaimNo { get; set; }

    public DateTime? ClaimDate { get; set; }

    public string? ChassisNo { get; set; }

    public int? SupplierId { get; set; }

    public int? JobCardHeaderId { get; set; }

    public int? CustomerLedgerId { get; set; }

    public int? RepairBillHeaderId { get; set; }

    public int? Ffirid { get; set; }

    public string? ClaimAccount { get; set; }

    public bool IsWjcclaimApproved { get; set; }

    public string? ApprovedEmpCode { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual LedgerMaster? CustomerLedger { get; set; }

    public virtual Ffirheader? Ffir { get; set; }

    public virtual JobCardHeader? JobCardHeader { get; set; }

    public virtual RepairBillHeader? RepairBillHeader { get; set; }

    public virtual LedgerMaster? Supplier { get; set; }

    public virtual ICollection<WarrantyJcclaimDetail> WarrantyJcclaimDetails { get; set; } = new List<WarrantyJcclaimDetail>();
}
