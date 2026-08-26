namespace DMS_BAPL_Data.DBModels;

public partial class ImpersonationLog
{
    public int Id { get; set; }
    public string SuperAdminUserId { get; set; } = null!;
    public string SuperAdminEmail { get; set; } = null!;
    public string TargetDealerUserId { get; set; } = null!;
    public string? TargetDealerCode { get; set; }
    public string TargetDealerEmail { get; set; } = null!;
    public DateTime StartedDate { get; set; }
    public DateTime? EndedDate { get; set; }
}