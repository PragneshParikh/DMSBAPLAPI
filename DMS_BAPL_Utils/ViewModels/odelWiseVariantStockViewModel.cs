using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // Pivoted row: one row per Model, one column per Colour Variant
    public class ModelWiseVariantStockPivotRow
    {
        public string ModelName { get; set; } = string.Empty;
        public Dictionary<string, int> VariantCounts { get; set; } = new();
        public int Total { get; set; }
    }

    public class ModelWiseVariantStockPivotResponse
    {
        public List<string> VariantNames { get; set; } = new();
        public List<ModelWiseVariantStockPivotRow> Rows { get; set; } = new();
        public Dictionary<string, int> ColumnTotals { get; set; } = new();
        public int GrandTotal { get; set; }
    }
}