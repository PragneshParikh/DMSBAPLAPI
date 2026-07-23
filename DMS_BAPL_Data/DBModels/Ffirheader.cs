using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class Ffirheader
{
    public int Id { get; set; }

    public string Ffirprefix { get; set; } = null!;

    public string? DealerCode { get; set; }

    public int Cirno { get; set; }

    public DateTime? Cirdate { get; set; }

    public int JobCardCustomerId { get; set; }

    public int JobCardHeaderId { get; set; }

    public string? PurposeOfCir { get; set; }

    public string FfirchassisNo { get; set; } = null!;

    public DateTime? FailureDate { get; set; }

    public string? ReportTitle { get; set; }

    public string? ReportPreparedBy { get; set; }

    public int? NoOfPassenger { get; set; }

    public string? TypeOfRoadSurface { get; set; }

    public bool RepeatFailure { get; set; }

    public bool ChassisModified { get; set; }

    public string? Ffirremarks { get; set; }

    public string? Ffirstatus { get; set; }

    public bool IsDelete { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<FfirdetailObservation> FfirdetailObservations { get; set; } = new List<FfirdetailObservation>();

    public virtual JobCardCustomer JobCardCustomer { get; set; } = null!;

    public virtual JobCardHeader JobCardHeader { get; set; } = null!;

    public virtual ICollection<MainPartAffectedFfir> MainPartAffectedFfirs { get; set; } = new List<MainPartAffectedFfir>();

    public virtual ICollection<WarrantyJcclaim> WarrantyJcclaims { get; set; } = new List<WarrantyJcclaim>();
}
