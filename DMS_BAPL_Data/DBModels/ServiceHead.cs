using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ServiceHead
{
    public int Id { get; set; }

    public int? JobTypeId { get; set; }

    public string? ServiceHeadName { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<JobCardHeader> JobCardHeaders { get; set; } = new List<JobCardHeader>();

    public virtual JobType? JobType { get; set; }

    public virtual ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
}
