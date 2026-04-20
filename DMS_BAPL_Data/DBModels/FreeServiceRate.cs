using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class FreeServiceRate
{
    public int Id { get; set; }

    public int OemmodelId { get; set; }

    public string? ServiceName { get; set; }

    public decimal? MetroRate { get; set; }

    public decimal? MetroGst { get; set; }

    public decimal? NonMetroRate { get; set; }

    public decimal? NonMetroGst { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
