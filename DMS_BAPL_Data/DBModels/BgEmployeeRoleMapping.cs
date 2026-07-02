using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class BgEmployeeRoleMapping
{
    public int Id { get; set; }

    public int BgEmployeeId { get; set; }

    public string Category { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual BgEmployeeMaster BgEmployee { get; set; } = null!;
}
