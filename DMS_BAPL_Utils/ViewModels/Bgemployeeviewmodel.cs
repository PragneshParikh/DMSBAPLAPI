using System;

namespace DMS_BAPL_Utils.ViewModels
{
    public class BgEmployeeViewModel
    {
        public int Id { get; set; }

        // ---- Personal ----------------------------------------
        public string? EmployeeCode { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? Mobile { get; set; }

        // ---- Address -----------------------------------------
        public int State { get; set; }
        public int City { get; set; }
        public string? Pincode { get; set; }

        // ---- Employment --------------------------------------
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfJoin { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string? ReportingTo { get; set; }
        public bool IsActive { get; set; } = true;

        // ---- Department --------------------------------------
        public int? Department { get; set; }

        // ---- Profile -----------------------------------------
        public int? ProfileId { get; set; }

        // ---- Login -------------------------------------------
        public string? EmailId { get; set; }
        public string? Password { get; set; }

        // ---- Zones -------------------------------------------
        //public string? Zones { get; set; }
        public string MappedZones { get; set; }
        public string MappedZoneIds { get; set; }

        // ---- Misc --------------------------------------------
        public string? ProfileImage { get; set; }
        public string? DealerCode { get; set; }
        public string? LocationCode { get; set; }

        // ---- Audit -------------------------------------------
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}