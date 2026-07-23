namespace DMS_BAPL_Data.DBModels
{
    public class EstimateDetail
    {
        public int Id { get; set; }
        public int EstimateHeaderId { get; set; }

        public string ItemType { get; set; } = string.Empty; // "Part" or "Labour"
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }

        public decimal Qty { get; set; } = 1;
        public decimal Rate { get; set; }

        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }

        public decimal Amount { get; set; }

        // Navigation property
        public EstimateHeader? EstimateHeader { get; set; }
    }
}