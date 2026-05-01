using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class State
{
    public int StateId { get; set; }

    public string StateName { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual ICollection<LedgerMaster> LedgerMasters { get; set; } = new List<LedgerMaster>();
}
