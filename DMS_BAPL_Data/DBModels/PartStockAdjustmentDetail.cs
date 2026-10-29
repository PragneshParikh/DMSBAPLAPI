namespace DMS_BAPL_Data.DBModels;

public partial class PartStockAdjustmentDetail
{
    public int Id { get; set; }
    public int AdjustmentId { get; set; }
    public string PartNo { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal? RateStock { get; set; }
    public int CurrentStock { get; set; }
    public int UpdateStock { get; set; }
    public int UpQuantityStg { get; set; }
    public int FinalQty { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public virtual PartStockAdjustment Adjustment { get; set; } = null!;
}
