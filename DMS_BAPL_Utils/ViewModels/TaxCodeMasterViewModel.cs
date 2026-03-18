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
}
