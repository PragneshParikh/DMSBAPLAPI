using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ZoneMaster
{
    public int Id { get; set; }

    public string? Zone { get; set; }

    public int? CityId { get; set; }

    public int? StateId { get; set; }

    public int? DealerId { get; set; }

    public bool IsActive { get; set; }
}
