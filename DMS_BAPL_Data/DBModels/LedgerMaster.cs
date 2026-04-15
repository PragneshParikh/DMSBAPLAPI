using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class LedgerMaster
{
    public int Id { get; set; }

    public string? LedgerCode { get; set; }

    public string? LedgerName { get; set; }

    public string? LedgerType { get; set; }

    public string? Gstno { get; set; }

    public string? Pan { get; set; }

    public string? AadharNumber { get; set; }

    public string? MobileNumber { get; set; }

    public string? Address { get; set; }

    public int? City { get; set; }

    public int? State { get; set; }

    public string? Pin { get; set; }

    public string? EMail { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual City? CityNavigation { get; set; }

    public virtual State? StateNavigation { get; set; }
}
