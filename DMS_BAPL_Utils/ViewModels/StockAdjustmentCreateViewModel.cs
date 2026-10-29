using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class StockAdjustmentCreateViewModel
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [Required]
        [JsonPropertyName("adjustmentDate")]
        public DateTime? AdjustmentDate { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("locationCode")]
        public string? LocationCode { get; set; }

        [StringLength(100)]
        [JsonPropertyName("dealerCode")]
        public string? DealerCode { get; set; }

        [StringLength(1000)]
        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }

        [Required]
        [MinLength(1)]
        [JsonPropertyName("items")]
        public List<StockAdjustmentItemCreateViewModel>? Items { get; set; }
    }

    public class StockAdjustmentItemCreateViewModel
    {
        [Required]
        [StringLength(500)]
        [JsonPropertyName("partNo")]
        public string? PartNo { get; set; }

        [StringLength(200)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("currentStock")]
        public int? CurrentStock { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("updateStock")]
        public int? UpdateStock { get; set; }

        [Required]
        [JsonPropertyName("upQuantityStg")]
        public int? UpQuantityStg { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("finalQty")]
        public int? FinalQty { get; set; }
    }

    public class StockAdjustmentSaveResponseViewModel
    {
        public int AdjustmentId { get; set; }

        [JsonPropertyName("prefixNo")]
        public string PrefixNo { get; set; } = string.Empty;
        [JsonPropertyName("srNo")]
        public int SrNo { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime AdjustmentDate { get; set; }
        public string DealerCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public List<StockAdjustmentItemResponseViewModel> Items { get; set; } = new();
    }

    public class StockAdjustmentItemResponseViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("partNo")]
        public string PartNo { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("rateStock")]
        public decimal? RateStock { get; set; }
        [JsonPropertyName("currentStock")]
        public int CurrentStock { get; set; }
        [JsonPropertyName("updateStock")]
        public int UpdateStock { get; set; }
        [JsonPropertyName("upQuantityStg")]
        public int UpQuantityStg { get; set; }
        [JsonPropertyName("finalQty")]
        public int FinalQty { get; set; }
    }
}
