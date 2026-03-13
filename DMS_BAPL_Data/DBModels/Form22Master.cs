using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class Form22Master
{
    public int Id { get; set; }

    public string? OemModelName { get; set; }

    public string? SoundLevelHorn { get; set; }

    public string? PassbyNoiseLevel { get; set; }

    public string? ApprovalCertificateNo { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? OemmodelId { get; set; }

    public bool? Isactive { get; set; }

    public virtual OemmodelMaster? Oemmodel { get; set; }
}
