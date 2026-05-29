using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ServiceType
{
    public int Id { get; set; }

    public int? ServiceHeadId { get; set; }

    public string? ServiceTypeName { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<JobCardHeader> JobCardHeaders { get; set; } = new List<JobCardHeader>();

    public virtual ICollection<LabourMaster> LabourMasters { get; set; } = new List<LabourMaster>();

    public virtual ICollection<ModelwiseServiceSchedule> ModelwiseServiceSchedules { get; set; } = new List<ModelwiseServiceSchedule>();

    public virtual ICollection<PartWiseLabourMaster> PartWiseLabourMasters { get; set; } = new List<PartWiseLabourMaster>();

    public virtual ServiceHead? ServiceHead { get; set; }
}
