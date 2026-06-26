using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ZoneMaster
{
    public int Id { get; set; }

    public string ZoneName { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<BgEmployeeZoneMapping> BgEmployeeZoneMappings { get; set; } = new List<BgEmployeeZoneMapping>();
}
