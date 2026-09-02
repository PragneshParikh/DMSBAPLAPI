using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class HSNCodeMasterViewModel
    {
        public string Hsncode { get; set; } = null!;

        public string? Description { get; set; }

        public string Type { get; set; } = null!;
    }

    public class HSNImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int InsertedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public List<HSNImportRowError> Errors { get; set; } = new List<HSNImportRowError>();
    }

    /// <summary>
    /// A single row-level outcome worth surfacing while importing HSN/SAC codes — used
    /// both for hard failures (validation/exception) and informational skips (duplicate code).
    /// </summary>
    public class HSNImportRowError
    {
        /// <summary>1-based row number as it appears in the Excel sheet (header row = row 1).</summary>
        public int RowNumber { get; set; }
        public string? HsnCode { get; set; }
        public string Message { get; set; } = null!;
    }
}
