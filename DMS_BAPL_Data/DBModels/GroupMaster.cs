using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class GroupMaster
{
    public int Id { get; set; }

    public string? GroupName { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
