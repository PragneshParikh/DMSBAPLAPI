using System;

namespace DMS_BAPL_Data.DBModels;

public partial class DmsZeroHoursJobData
{
    public int Id { get; set; }
    public string? CompName { get; set; }
    public string? DealerCode { get; set; }
    public string? DealerState { get; set; }
    public DateOnly? JobDate { get; set; }
    public string? JobNo { get; set; }
    public string? JobCategory { get; set; }
    public decimal? FfrPer { get; set; }
    public string? DocNo { get; set; }
    public DateOnly? DocDate { get; set; }
    public string? DocType { get; set; }
    public string? JobStatus { get; set; }
    public string? Location { get; set; }
    public string? Model { get; set; }
    public string? BrandName { get; set; }
    public string? PartyName { get; set; }
    public string? PartyMobile { get; set; }
    public string? Supervisor { get; set; }
    public string? Technician { get; set; }
    public decimal? Kms { get; set; }
    public string? ServiceHead { get; set; }
    public string? JobType { get; set; }
    public string? RegNo { get; set; }
    public string? VehicleType { get; set; }
    public string? EngineNo { get; set; }
    public string? ChassisNo { get; set; }
    public DateOnly? ExpectedDeliveryDate { get; set; }
    public DateOnly? ProformaDate { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public decimal? EstimatedJobExpenses { get; set; }
    public decimal? LabourHours { get; set; }
    public decimal? Parts { get; set; }
    public decimal? Accessory { get; set; }
    public decimal? Oil { get; set; }
    public decimal? Labour { get; set; }
    public decimal? OutsideWork { get; set; }
    public decimal? TotalWoTax { get; set; }
    public decimal? GstAmnt { get; set; }
    public decimal? IgstAmnt { get; set; }
    public decimal? NetTotal { get; set; }
    public string? InTime { get; set; }
    public string? CloseTime { get; set; }
    public DateOnly? CloseDate { get; set; }
    public string? CopnNo { get; set; }
    public DateOnly? SaleDate { get; set; }
    public string? BatterySerialNo1 { get; set; }
    public string? BatterySerialNo2 { get; set; }
    public string? BatterySerialNo3 { get; set; }
    public string? BatterySerialNo4 { get; set; }
    public string? BatterySerialNo5 { get; set; }
    public string? BatterySerialNo6 { get; set; }
    public string? IndividualAhBattery1 { get; set; }
    public string? IndividualAhBattery2 { get; set; }
    public string? IndividualAhBattery3 { get; set; }
    public string? IndividualAhBattery4 { get; set; }
    public string? IndividualAhBattery5 { get; set; }
    public string? IndividualAhBattery6 { get; set; }
    public string? ChargerNo { get; set; }
    public string? ControllerNo { get; set; }
    public string? Converter { get; set; }
    public string? CreateThrough { get; set; }
    public string? ZoneName { get; set; }
    public string? UniqueId { get; set; }
    public string? DealerType { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
}