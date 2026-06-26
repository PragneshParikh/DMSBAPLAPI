using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class EmployeeProfileMaster
{
    public int Id { get; set; }

    public string ProfileName { get; set; } = null!;

    public int SortOrder { get; set; }

    public virtual ICollection<BgEmployeeProfileMapping> BgEmployeeProfileMappings { get; set; } = new List<BgEmployeeProfileMapping>();
}
