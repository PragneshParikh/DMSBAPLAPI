using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class JobType
{
    public int Id { get; set; }

    public string? JobTypeName { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<JobCardHeader> JobCardHeaders { get; set; } = new List<JobCardHeader>();
}
