using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class CounterBillHeader
{
    public int Id { get; set; }

    public string BillNo { get; set; } = null!;

    public DateTime BillDate { get; set; }

    public string BillType { get; set; } = null!;

    public string LocCode { get; set; } = null!;

    public string? CashCreditAcc { get; set; }

    public string? PartyName { get; set; }

    public int? PartyState { get; set; }

    public string? ChassisNo { get; set; }

    public decimal BillAmount { get; set; }

    public string? Remarks { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<CounterBillDetail> CounterBillDetails { get; set; } = new List<CounterBillDetail>();
}
