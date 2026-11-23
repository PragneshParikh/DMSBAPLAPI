namespace DMS_BAPL_Data.DBModels;

public partial class DmsJobReport
{
    public int Id { get; set; }

    public string? DealerName { get; set; }
    public string? DealerCode { get; set; }
    public string? DealerLocation { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? JobNo { get; set; }
    public DateOnly? JobDate { get; set; }
    public string? JobType { get; set; }
    public string? ServiceHead { get; set; }
    public string? ServiceType { get; set; }
    public string? Kms { get; set; }
    public string? CustomerName { get; set; }
    public string? MobileNo { get; set; }
    public string? ChassisNo { get; set; }
    public string? RegNo { get; set; }
    public string? EngineNo { get; set; }
    public string? ItemName { get; set; }
    public string? CustomerVoice { get; set; }
    public string? ComplaintCode { get; set; }
    public string? Observation { get; set; }
    public string? SupervisorComment { get; set; }
    public string? JobStatus { get; set; }
    public string? BatteryNo { get; set; }
    public string? BrandName { get; set; }
    public string? ChargerNo { get; set; }
    public DateOnly? SaleDate { get; set; }
    public string? Supervisor { get; set; }
    public string? Technician { get; set; }
    public DateOnly? JobEndDate { get; set; }
    public string? CreatedThrough { get; set; }
    public string? UniqueKey { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Action { get; set; }
}