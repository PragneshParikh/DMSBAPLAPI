using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class TaxCodeMasterViewModel
    {
        public int Id { get; set; }
        public string TaxCode { get; set; }
        public string? Description { get; set; }
        public decimal TaxRate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class TaxCodeImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int InsertedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public List<TaxCodeImportRowError> Errors { get; set; } = new List<TaxCodeImportRowError>();
    }

    /// <summary>
    /// A single row-level outcome worth surfacing while importing Tax Code Master rows —
    /// used both for hard failures (validation) and informational skips (exact duplicate).
    /// </summary>
    public class TaxCodeImportRowError
    {
        /// <summary>1-based row number as it appears in the Excel sheet (header row = row 1).</summary>
        public int RowNumber { get; set; }
        public string? TaxCode { get; set; }
        public string Message { get; set; } = null!;
    }
}
