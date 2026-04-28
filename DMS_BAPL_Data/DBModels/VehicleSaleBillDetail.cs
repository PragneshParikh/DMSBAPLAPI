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

    public decimal? Sgstper { get; set; }

    public decimal? Sgstamnt { get; set; }

    public decimal? Cgstper { get; set; }

    public decimal? Cgstamnt { get; set; }

    public decimal? Igstper { get; set; }

    public decimal? Igstamnt { get; set; }

    public int? MfgYear { get; set; }

    public string? InsNo { get; set; }

    public DateOnly? InsStartDate { get; set; }

    public string? RegNo { get; set; }

    public DateOnly? InsExpDate { get; set; }

    public string? ModelName { get; set; }

    public string? Colour { get; set; }

    public string? Battery { get; set; }

    public string? ConvertorNo { get; set; }

    public string? ChargerNo { get; set; }

    public string? ControllerNo { get; set; }

    public string? Key { get; set; }

    public string? BookNo { get; set; }

    public string? ExtWarranty { get; set; }

    public string? BatteryChemical { get; set; }

    public string? BatteryCapacity { get; set; }

    public string? BatteryMake { get; set; }

    public string? StockDetailsNo { get; set; }

    public string? Vcu { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual VehicleSaleBillHeader VehicleSaleBill { get; set; } = null!;
}
