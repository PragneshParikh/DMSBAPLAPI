using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class LmsleadMaster
{
    public int Id { get; set; }

    public int Leadid { get; set; }

    public string Name { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public string? Email { get; set; }

    public DateTime Date { get; set; }

    public string? Area { get; set; }

    public string City { get; set; } = null!;

    public string Brancharea { get; set; } = null!;

    public string Company { get; set; } = null!;

    public int? Pincode { get; set; }

    public TimeOnly? Time { get; set; }

    public int Branchpin { get; set; }

    public string Model { get; set; } = null!;

    public string Variant { get; set; } = null!;

    public int? DealerId { get; set; }

    public string Dealercode { get; set; } = null!;

    public string? Productcode { get; set; }

    public string Sourceapp { get; set; } = null!;

    public int? ColorId { get; set; }

    public string? Color { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public virtual ColorMaster? ColorNavigation { get; set; }

    public virtual DealerMaster? Dealer { get; set; }
}
