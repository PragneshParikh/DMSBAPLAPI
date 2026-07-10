using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // One row per dealer in the pivoted Model Wise Sale Report.
    public class ModelWiseSalePivotRow
    {
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public Dictionary<string, int> ModelCounts { get; set; } = new();

        // Row total — horizontal sum across every model for this dealer.
        public int Total { get; set; }
    }
    public class ModelWiseSalePivotResponse
    {
        // Distinct model names across the filtered data, in column display order.
        public List<string> ModelNames { get; set; } = new();

        public List<ModelWiseSalePivotRow> Rows { get; set; } = new();

        // Column totals — key = model name, value = sum across all dealers.
        public Dictionary<string, int> ColumnTotals { get; set; } = new();

        // Grand total — sum of every row's Total (== sum of every column total).
        public int GrandTotal { get; set; }
    }
}