using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ErpVehicleSale
{
    public int Id { get; set; }

    public string? DealerName { get; set; }

    public string? DealerCode { get; set; }

    public string? InvoiceNo { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    public string? Location { get; set; }

    public string? LocCode { get; set; }

    public string? LocationCity { get; set; }

    public DateOnly? CustDob { get; set; }

    public string? Gender { get; set; }

    public string? SoldTo { get; set; }

    public string? AccountType { get; set; }

    public string? PartyEmail { get; set; }

    public string? CusMob { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ExecutiveName { get; set; }

    public string? Pin { get; set; }

    public string? ChassisNo { get; set; }

    public string? MotorNo { get; set; }

    public string? Remarks { get; set; }

    public string? ItemModel { get; set; }

    public string? Oemmodel { get; set; }

    public string? ColorCode { get; set; }

    public string? VehicleType { get; set; }

    public string? VehicleGroup { get; set; }

    public string? Hsnsaccode { get; set; }

    public string? SaleType { get; set; }

    public string? FinancedBy { get; set; }

    public decimal FinAmount { get; set; }

    public decimal ItemRate { get; set; }

    public decimal InsuAmount { get; set; }

    public decimal RegnAmount { get; set; }

    public decimal AcsryAmount { get; set; }

    public decimal PreGstdiscAmount { get; set; }

    public string? DiscTypeName { get; set; }

    public decimal PostGstdisc { get; set; }

    public decimal FameIi { get; set; }

    public decimal StateFameIi { get; set; }

    public decimal Sgstper { get; set; }

    public decimal Sgstamount { get; set; }

    public decimal Cgstper { get; set; }

    public decimal Cgstamount { get; set; }

    public decimal Igstper { get; set; }

    public decimal Igstamount { get; set; }

    public decimal NetAmount { get; set; }

    public string? ReferenceNo { get; set; }

    public DateOnly? BookingDate { get; set; }

    public string? TotalCount { get; set; }

    public string? Battery { get; set; }

    public string? BatteryChemical { get; set; }

    public string? BatteryCapacity { get; set; }

    public string? BatteryMake { get; set; }

    public string? ChargerNo { get; set; }

    public string? ChargerNo2 { get; set; }

    public string? Converter { get; set; }

    public string? Vcu { get; set; }

    public string? ControllerNo { get; set; }

    public string? FameIirequired { get; set; }

    public string? SegmentName { get; set; }

    public string? InstitutionalName { get; set; }

    public string? SchemeName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }
}
