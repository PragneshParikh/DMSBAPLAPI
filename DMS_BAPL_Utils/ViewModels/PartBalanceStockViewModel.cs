namespace DMS_BAPL_Utils.ViewModels
{
    public class PartBalanceStockViewModel
    {
        public string PartNo { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BalanceStock { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string? LocationName { get; set; }
        public decimal? RateStock { get; set; }
    }
}
