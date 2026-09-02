using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class HsnwiseTaxCodeViewModel
    {

        public int Id { get; set; }

        public string Hsncode { get; set; } = null!;

        public string AtaxCode { get; set; } = null!;

        public string StateFlag { get; set; } = null!;

        public DateTime EffectiveDate { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    public class HSNCodeList 
    {
        public string HsnCodeDD { get; set; }
    }

    public class HsnwiseTaxImportResultViewModel
    {
        public int TotalRows { get; set; }

        public int InsertedCount { get; set; }

        public int UpdatedCount { get; set; } = 0;

        public int SkippedCount { get; set; }

        public int FailedCount { get; set; }

        public List<HsnwiseTaxImportRowError> Errors { get; set; }
            = new List<HsnwiseTaxImportRowError>();
    }
    public class HsnwiseTaxImportRowError
    {
        public int RowNumber { get; set; }

        public string? Hsncode { get; set; }

        public string? AtaxCode { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
