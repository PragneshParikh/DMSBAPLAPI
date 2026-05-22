using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ColorMaster
{
    public int Id { get; set; }

    public int Rrgcoloridno { get; set; }

    public string Colorname { get; set; } = null!;

    public string Colorcode { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
