using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class CounterBillHeaderAuditLog
{
    public int AuditId { get; set; }

    public string? AuditAction { get; set; }

    public string? AuditUser { get; set; }

    public DateTime? AuditTimestamp { get; set; }

    public int? Id { get; set; }

    public string? BillNo { get; set; }

    public DateTime? BillDate { get; set; }

    public string? BillType { get; set; }

    public string? LocCode { get; set; }

    public string? CashCreditAcc { get; set; }

    public string? PartyName { get; set; }

    public int? PartyState { get; set; }

    public string? ChassisNo { get; set; }

    public decimal? BillAmount { get; set; }

    public string? Remarks { get; set; }

    public int? CustomerLedgerId { get; set; }

    public string? DealerCode { get; set; }

    public string? MobileNo { get; set; }

    public int? IsDeleted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
