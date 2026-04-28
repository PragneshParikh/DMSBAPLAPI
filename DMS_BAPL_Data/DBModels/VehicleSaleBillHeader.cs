using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class VehicleSaleBillHeader
{
    public int Id { get; set; }

    public DateTime SaleDate { get; set; }

    public string SaleBillNo { get; set; } = null!;

    public bool IsD2d { get; set; }

    public string? CustomerType { get; set; }

    public string? Location { get; set; }

    public string? SaleType { get; set; }

    public string? CashAccount { get; set; }

    public string? Financier { get; set; }

    public string? BillType { get; set; }

    public string? BillFrom { get; set; }

    public string? CustomerName { get; set; }

    public string? BillingName { get; set; }

    public string? SalesExecutive { get; set; }

    public string? TempRegNo { get; set; }

    public string? BookingId { get; set; }

    public string? PrintType { get; set; }

    public string? RefName { get; set; }

    public string? RefAddress { get; set; }

    public string? RefEmail { get; set; }

    public int? RefPoint { get; set; }

    public string? RefRemarks { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Erpstatus { get; set; }

    public int? LedgerId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? DealerCode { get; set; }

    public virtual ICollection<VehicleSaleBillDetail> VehicleSaleBillDetails { get; set; } = new List<VehicleSaleBillDetail>();
}
