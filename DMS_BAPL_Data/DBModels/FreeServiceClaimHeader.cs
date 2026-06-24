using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class FreeServiceClaimHeader
{
    public int Id { get; set; }

    public string? ClaimPrefix { get; set; }

    public string? ClaimNo { get; set; }

    public DateTime? ClaimDate { get; set; }

    public string? DealerCode { get; set; }

    public string? LocationCode { get; set; }

    public string? Remarks { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public string? UpdatedDate { get; set; }

    public virtual ICollection<FreeServiceClaimDetail> FreeServiceClaimDetails { get; set; } = new List<FreeServiceClaimDetail>();
}
