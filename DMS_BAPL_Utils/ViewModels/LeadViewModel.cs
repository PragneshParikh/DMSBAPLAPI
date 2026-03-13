using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class LeadViewModel
    {
        public int Id { get; set; }

        public int leadid { get; set; }

        public string CustomerName { get; set; } = null!;

        public string CustomerMobile { get; set; } = null!;

        public string? CustomerEmail { get; set; }

        public DateTime LMSDate { get; set; }

        public string? LMSArea { get; set; }

        public string LMSCity { get; set; } = null!;

        public string LMSBrancharea { get; set; } = null!;

        public string CustomerCompany { get; set; } = null!;

        public int? LMSPincode { get; set; }

        public TimeOnly? CustomerTime { get; set; }

        public int LMSBranchpin { get; set; }

        public string LMSModel { get; set; } = null!;

        public string LMSVariant { get; set; } = null!;

        public int? DealerId { get; set; } = null!;

        public string Dealercode { get; set; } = null!;

        public string Productcode { get; set; }

        public string Sourceapp { get; set; } = null!;

        public int? ColorId { get; set; } = null!;

        public string? Color { get; set; } = null!;

        public string createdby { get; set; }
        public DateTime createddatetime { get; set; }
        public string  updatedby { get; set; }
        public DateTime? updateddatetime { get; set; }
    }
}
