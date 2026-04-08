using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class State
{
    public int StateId { get; set; }

    public string StateName { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime Createddate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? Updateddate { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
