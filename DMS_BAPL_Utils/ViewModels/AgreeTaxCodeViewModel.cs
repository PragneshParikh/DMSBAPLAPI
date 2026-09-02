using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class AgreeTaxCodeViewModel
    {
        public int Id { get; set; }

        public string AtaxCode { get; set; } = null!;

        public string? Description { get; set; }

        public List<TaxDetailViewModel> TaxDetails { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    public class TaxDetailViewModel
    {
        public int SrNo { get; set; }
        public string TaxCode { get; set; }
        public decimal TaxRate { get; set; }
    }

    public class TaxCodeWithRateViewModel
    {
        public string TaxCode { get; set; }
        public decimal TaxRate { get; set; }
    }

    public class AggregateTaxImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int FailedCount { get; set; }
        public List<AggregateTaxImportRowError> Errors { get; set; } = new List<AggregateTaxImportRowError>();
    }

    /// <summary>
    /// A single row-level failure encountered while importing Aggregate Tax Code data.
    /// </summary>
    public class AggregateTaxImportRowError
    {
        /// <summary>1-based row number as it appears in the Excel sheet (header row = row 1).</summary>
        public int RowNumber { get; set; }
        public string? AtaxCode { get; set; }
        public string Message { get; set; } = null!;
    }
}
