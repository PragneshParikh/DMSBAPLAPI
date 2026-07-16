using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class LedgerDetailViewModel
    {
        public int Id { get; set; }

        public string? LedgerCode { get; set; }

        public string? LedgerName { get; set; }

        public string? LedgerType { get; set; }
        public string? DealerCode { get; set; }

        public string? Gstno { get; set; }

        public string? Pan { get; set; }

        public string? AadharNumber { get; set; }

        public string? MobileNumber { get; set; }
        public string? AltMobileNumber { get; set; }

        public string? Address { get; set; }

        public int? City { get; set; }

        public int? State { get; set; }

        public string? Pin { get; set; }

        public string? EMail { get; set; }

        public string? Gender { get; set; }
        public int? OccupationId { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? cityName { get; set; }
        public string? stateName { get; set; }
        public bool? D2DProvision { get; set; }
    }

    public class LedgerExcelViewModel
    {

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? LedgerCode { get; set; }
        public string? LedgerName { get; set; }
        public string? LedgerType { get; set; }
        public string? Gstno { get; set; }
        public string? Pan { get; set; }

        public string? AadharNumber { get; set; }

        public string? MobileNumber { get; set; }

        public string? Address { get; set; }

        public string? Pin { get; set; }

        public string? EMail { get; set; }

        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        public string? cityName { get; set; }
        public string? stateName { get; set; }
    }
}
