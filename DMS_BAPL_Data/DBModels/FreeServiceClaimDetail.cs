using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class FreeServiceClaimDetail
{
    public int Id { get; set; }

    public int HeaderClaimId { get; set; }

    public int JobId { get; set; }

    public bool? IsApproved { get; set; }

    public string? ApprovedRejectBy { get; set; }

    public DateTime? ApprovedRejectDate { get; set; }

    public string? RejectReason { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual FreeServiceClaimHeader HeaderClaim { get; set; } = null!;
}
