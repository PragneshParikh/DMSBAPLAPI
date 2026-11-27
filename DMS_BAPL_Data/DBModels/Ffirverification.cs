namespace DMS_BAPL_Data.DBModels;

public partial class Ffirverification
{
    public int FfirId { get; set; }

    public string Status { get; set; } = "Pending";

    public string? Remarks { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }
}
