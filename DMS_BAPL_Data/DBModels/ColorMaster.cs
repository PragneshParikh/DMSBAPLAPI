using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ColorMaster
{
    public int Id { get; set; }

    public int Rrgcoloridno { get; set; }

    public string Colorname { get; set; } = null!;

    public string Colorcode { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<LmsleadMaster> LmsleadMasters { get; set; } = new List<LmsleadMaster>();
}
