using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class JobCardComplaint
{
    public int Id { get; set; }

    public string? DealerCode { get; set; }

    public int JobCardHeaderId { get; set; }

    public string? CustomerVoice { get; set; }

    public string? ComplaintCode { get; set; }

    public string? Complaint { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public virtual JobCardHeader JobCardHeader { get; set; } = null!;
}
