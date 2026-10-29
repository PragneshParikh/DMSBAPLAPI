using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class StockAdjustmentNumberSequenceViewModel
    {
        [JsonPropertyName("dealerCode")]
        public string DealerCode { get; set; } = string.Empty;

        [JsonPropertyName("prefix")]
        public string Prefix { get; set; } = string.Empty;

        [JsonPropertyName("nextNumber")]
        public int NextNumber { get; set; }

        [JsonPropertyName("generatedNumber")]
        public string GeneratedNumber { get; set; } = string.Empty;
    }
}
