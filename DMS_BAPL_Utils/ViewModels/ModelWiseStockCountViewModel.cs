using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // One row per dealer in the pivoted Model-wise Current Stock report.
    public class ModelWiseStockPivotRow
    {
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }

        // Key = model name, Value = total stock count (Available + Allocated
        // combined) of that model held by this dealer. Always has one entry
        // per name in ModelWiseStockPivotResponse.ModelNames (0 where none).
        public Dictionary<string, int> ModelCounts { get; set; } = new();

        // Row total — horizontal sum across every model for this dealer.
        public int Total { get; set; }
    }

    // Backs GET api/Report/model-wise-stock-count.
    // Pivoted layout: Dealer Name rows x Model Name columns, with a Total
    // column per row and a Total row (ColumnTotals) summing each model
    // vertically across all dealers. Mirrors ModelWiseSalePivotResponse.
    public class ModelWiseStockPivotResponse
    {
        public List<string> ModelNames { get; set; } = new();

        public List<ModelWiseStockPivotRow> Rows { get; set; } = new();

        public Dictionary<string, int> ColumnTotals { get; set; } = new();

        public int GrandTotal { get; set; }
    }
}