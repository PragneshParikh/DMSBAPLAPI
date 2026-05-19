using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class JobCardCustomer
{
    public int Id { get; set; }

    public int JobCardHeaderId { get; set; }

    public int? CustomerLedgerId { get; set; }

    public int? VehicleSaleBillid { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerMobile { get; set; }

    public string? CustomerAltMobile { get; set; }

    public string? ModelName { get; set; }

    public string? ChassisNo { get; set; }

    public string? RegisterNo { get; set; }

    public string? MotorNo { get; set; }

    public string? BatteryNo { get; set; }

    public DateTime? SaleDate { get; set; }

    public DateTime? InsuranceExpDate { get; set; }

    public DateTime? NextserviceDueDate { get; set; }

    public DateTime? RsarenewalDate { get; set; }

    public string? Remarks { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual LedgerMaster? CustomerLedger { get; set; }

    public virtual ICollection<Ffirheader> Ffirheaders { get; set; } = new List<Ffirheader>();

    public virtual JobCardHeader JobCardHeader { get; set; } = null!;

    public virtual VehicleSaleBillHeader? VehicleSaleBill { get; set; }
}
