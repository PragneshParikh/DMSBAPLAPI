using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class TermConditionMasterViewModel
    {
        public int ConditionId { get; set; }
        public string? TermCondition { get; set; } = string.Empty;
        public int? ConditionModule { get; set; }
        public DateTime? ConditionEffectiveDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
