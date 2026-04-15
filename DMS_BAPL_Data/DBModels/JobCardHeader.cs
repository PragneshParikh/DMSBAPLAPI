using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class JobCardHeader
{
    public int Id { get; set; }

    public string? DealerCode { get; set; }

    public int? Jobtype { get; set; }

    public string? Chassisno { get; set; }

    public int? Vehiclekms { get; set; }

    public int? Servicehead { get; set; }

    public int? Servicetype { get; set; }

    public string? Serviceloc { get; set; }

    public string? Couponno { get; set; }

    public string? Jobprefix { get; set; }

    public DateOnly? JobinDate { get; set; }

    public string? JobinTime { get; set; }

    public int? JobNo { get; set; }

    public int? ManualjobNo { get; set; }

    public DateOnly? EstdelDate { get; set; }

    public string? EstdelTime { get; set; }

    public int? JobSource { get; set; }

    public string? Supervisor { get; set; }

    public string? Technician { get; set; }

    public int? Jobestmate { get; set; }

    public int? AirpressureRearTyre { get; set; }

    public int? AirpressurefrontTyre { get; set; }

    public string? Observation { get; set; }

    public string? SupervisorComment { get; set; }

    public bool? IsPdiSuccess { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<JobCardBatteryDetail> JobCardBatteryDetails { get; set; } = new List<JobCardBatteryDetail>();

    public virtual ICollection<JobCardComplaint> JobCardComplaints { get; set; } = new List<JobCardComplaint>();

    public virtual ICollection<JobCardCustomer> JobCardCustomers { get; set; } = new List<JobCardCustomer>();

    public virtual JobSource? JobSourceNavigation { get; set; }

    public virtual JobType? JobtypeNavigation { get; set; }

    public virtual ICollection<PdichecklistChassisWise> PdichecklistChassisWises { get; set; } = new List<PdichecklistChassisWise>();

    public virtual ServiceHead? ServiceheadNavigation { get; set; }

    public virtual ServiceType? ServicetypeNavigation { get; set; }
}
