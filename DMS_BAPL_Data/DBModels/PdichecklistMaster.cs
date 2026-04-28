using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class PdichecklistMaster
{
    public int Id { get; set; }

    public string? PdiheadName { get; set; }

    public string? PdicheckName { get; set; }

    public string? Pdidescription { get; set; }

    public bool? Isactive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<PdichecklistChassisWise> PdichecklistChassisWises { get; set; } = new List<PdichecklistChassisWise>();
}
