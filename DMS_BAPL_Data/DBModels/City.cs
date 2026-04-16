using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class City
{
    public int CityId { get; set; }

    public string CityName { get; set; } = null!;

    public int StateId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime Createddate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? Updateddate { get; set; }

    public bool? IsMetro { get; set; }

    public int? TierLevel { get; set; }

    public string? Abbreviation { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<LedgerMaster> LedgerMasters { get; set; } = new List<LedgerMaster>();

    public virtual ICollection<LedgerMaster> LedgerMasters { get; set; } = new List<LedgerMaster>();

    public virtual State State { get; set; } = null!;
}
