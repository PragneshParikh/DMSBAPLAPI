using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class PdichecklistChassisWise
{
    public int Id { get; set; }

    public int PdichecklistMasterId { get; set; }

    public int? JobCardMasterId { get; set; }

    public bool? IsStatus { get; set; }

    public string? Remarks { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public virtual JobCardHeader? JobCardMaster { get; set; }

    public virtual PdichecklistMaster PdichecklistMaster { get; set; } = null!;
}
