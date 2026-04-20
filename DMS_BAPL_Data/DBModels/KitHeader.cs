using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class KitHeader
{
    public int Id { get; set; }

    public string KitName { get; set; } = null!;

    public DateTime KitDate { get; set; }

    public bool Status { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<KitDetail> KitDetails { get; set; } = new List<KitDetail>();
}
