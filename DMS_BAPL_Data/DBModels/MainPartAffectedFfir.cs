using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class MainPartAffectedFfir
{
    public int Id { get; set; }

    public int Ffirid { get; set; }

    public string? PartAffectedName { get; set; }

    public string? PartAffectedDescription { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Ffirheader Ffir { get; set; } = null!;
}
