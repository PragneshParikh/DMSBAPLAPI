using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class NumberSequenceViewModel
    {
        public string SequenceCode { get; set; } = null!;

        public string SequenceName { get; set; } = null!;

        public string Format { get; set; } = null!;

        public int NextNo { get; set; }

        public int Increment { get; set; }

        public string Year { get; set; } = null!;

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
    }
}
