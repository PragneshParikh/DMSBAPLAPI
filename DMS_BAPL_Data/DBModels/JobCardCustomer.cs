using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class JobCardCustomer
{
    public int Id { get; set; }

    public int JobCardHeaderId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerMobile { get; set; }

    public string? CustomerAltMobile { get; set; }

    public string? ModelName { get; set; }

    public string? ChassisNo { get; set; }

    public string? RegisterNo { get; set; }

    public string? MotorNo { get; set; }

    public string? BatteryNo { get; set; }

    public DateOnly? SaleDate { get; set; }

    public DateOnly? InsuranceExpDate { get; set; }

    public DateOnly? NextserviceDueDate { get; set; }

    public DateOnly? RsarenewalDate { get; set; }

    public string? Remarks { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public virtual JobCardHeader JobCardHeader { get; set; } = null!;
}
