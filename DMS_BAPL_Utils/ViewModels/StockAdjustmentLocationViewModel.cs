using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class StockAdjustmentPartViewModel
    {
        [JsonPropertyName("partNo")]
        public string PartNo { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("stockQty")]
        public int StockQty { get; set; }

        [JsonPropertyName("itemRate")]
        public decimal ItemRate { get; set; }

        [JsonPropertyName("rate")]
        public decimal Rate => ItemRate;
    }

    public class StockAdjustmentPartDetailsViewModel
    {
        [JsonPropertyName("partNo")]
        public string PartNo { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("currentStock")]
        public int CurrentStock { get; set; }

        [JsonPropertyName("rate")]
        public decimal Rate { get; set; }
    }

    public class StockAdjustmentLocationViewModel
    {
        [JsonPropertyName("locationId")]
        public int LocationId { get; set; }

        [JsonPropertyName("locationCode")]
        public string? LocationCode { get; set; }

        [JsonPropertyName("locationName")]
        public string LocationName { get; set; } = string.Empty;
    }


    public class StockAdjustmentListViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("adjustmentId")]
        public int AdjustmentId { get; set; }

        [JsonPropertyName("prefixNo")]
        public string? PrefixNo { get; set; }

        [JsonPropertyName("srNo")]
        public int? SrNo { get; set; }
        [JsonPropertyName("Rate")]
        public int? Rate { get; set; }

        [JsonPropertyName("adjustmentDate")]
        public DateTime AdjustmentDate { get; set; }

        [JsonPropertyName("dealerCode")]
        public string? DealerCode { get; set; }

        [JsonPropertyName("locationCode")]
        public string? LocationCode { get; set; }

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("partNo")]
        public string? PartNo { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("currentStock")]
        public decimal CurrentStock { get; set; }

        [JsonPropertyName("updateStock")]
        public decimal UpdateStock { get; set; }

        [JsonPropertyName("upQuantityStg")]
        public decimal UpQuantityStg { get; set; }

        [JsonPropertyName("finalQty")]
        public decimal FinalQty { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime? CreatedDate { get; set; }
    }

    public class StockAdjustmentEditViewModel
    {
        public int Id { get; set; }
        public string? PrefixNo { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string? LocationCode { get; set; }
        public string? Remarks { get; set; }

        public List<StockAdjustmentItemViewModel>? Items { get; set; }
    }

    public class StockAdjustmentItemViewModel
    {
        public string? PartNo { get; set; }

        public string? Description { get; set; }

        public decimal? RateStock { get; set; }

        public int? CurrentStock { get; set; }

        public int? UpdateStock { get; set; }

        public int? UpQuantityStg { get; set; }

        public int? FinalQty { get; set; }
    }

}
