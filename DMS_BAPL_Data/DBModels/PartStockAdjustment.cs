namespace DMS_BAPL_Data.DBModels;

public partial class PartStockAdjustment
{
    public int Id { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public string? PrefixNo { get; set; }
    public int? SrNo { get; set; }
    public string DealerCode { get; set; } = null!;
    public string LocationCode { get; set; } = null!;
    public string? Remarks { get; set; }
    public string Status { get; set; } = "Draft";
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<PartStockAdjustmentDetail> Details { get; set; } = new List<PartStockAdjustmentDetail>();
}
