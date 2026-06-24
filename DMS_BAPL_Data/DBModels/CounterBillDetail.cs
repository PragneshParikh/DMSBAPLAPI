using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class CounterBillDetail
{
    public int Id { get; set; }

    public int CounterBillId { get; set; }

    public string PartCode { get; set; } = null!;

    public string? SaleType { get; set; }

    public decimal Qty { get; set; }

    public decimal Rate { get; set; }

    public string? DiscType { get; set; }

    public decimal Discount { get; set; }

    public decimal Mrp { get; set; }

    public decimal Igstper { get; set; }

    public decimal Igstamnt { get; set; }

    public decimal Cgstper { get; set; }

    public decimal Cgstamnt { get; set; }

    public decimal Sgstper { get; set; }

    public decimal Sgstamnt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual CounterBillHeader CounterBill { get; set; } = null!;
}
