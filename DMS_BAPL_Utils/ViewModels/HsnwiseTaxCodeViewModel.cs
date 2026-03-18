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
}
