using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ComplaintMaster
{
    public int Id { get; set; }

    public string? ComplaintName { get; set; }

    public int? GroupName { get; set; }

    public bool? Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual GroupMaster? GroupNameNavigation { get; set; }
}
