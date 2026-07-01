using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ZoneMaster
{
    public int Id { get; set; }
    public string Zone { get; set; } = null!;  // mapped to "zone" column
    public int? CityId { get; set; }             // mapped to "city_id"
    public int? StateId { get; set; }             // mapped to "state_id"
    public int? DealerId { get; set; }             // mapped to "dealer_id"
    public bool IsActive { get; set; }             // mapped to "is_active"
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}