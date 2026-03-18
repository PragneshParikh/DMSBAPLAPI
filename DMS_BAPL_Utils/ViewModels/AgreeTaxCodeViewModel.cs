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
}
