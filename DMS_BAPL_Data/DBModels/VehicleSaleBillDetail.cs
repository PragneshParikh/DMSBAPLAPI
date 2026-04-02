using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class VehicleSaleBillDetail
{
    public int Id { get; set; }

    public int VehicleSaleBillId { get; set; }

    public string ChassisNo { get; set; } = null!;

    public decimal ItemRate { get; set; }

    public decimal? PreGstDiscount { get; set; }

    public decimal? RegAmount { get; set; }

    public decimal? InsuranceAmount { get; set; }

    public bool HasDevice { get; set; }

    public bool HasKit { get; set; }

    public bool IsDelivered { get; set; }

    public string? Segment { get; set; }

    public string? InstitutionalType { get; set; }

    public string? SchemeName { get; set; }

    public string? Narration { get; set; }

    public decimal FinalAmount { get; set; }

    public bool IsAgainstExchange { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual VehicleSaleBillHeader VehicleSaleBill { get; set; } = null!;
}
