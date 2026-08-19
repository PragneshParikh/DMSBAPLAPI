using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class BgEmployeeViewModel
    {
        public int Id { get; set; }
        public string? EmployeeCode { get; set; }
        public string? TsmCode { get; set; }
        public string? AreaOfficeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? Mobile { get; set; }
        public int? State { get; set; }
        public int? City { get; set; }
        public string? Pincode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfJoin { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? ReportingTo { get; set; }
        public bool IsActive { get; set; }
        public int? Department { get; set; }
        public int? ProfileId { get; set; }
        public string? EmailId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? MappedZones { get; set; }
        public string? MappedZoneIds { get; set; }
        public string? MappedEmployeeIds { get; set; }
        public string? MappedEmployees { get; set; }
        public string? ProfileImage { get; set; }
        public string? DealerCode { get; set; }
        public string? LocationCode { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public bool IsAccepted { get; set; }

        // ---- Role / Department selection ----------------------
        public List<string> SelectedDepartments { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<RoleMappingDto> RoleMappings { get; set; } = new();
    }

    public class RoleMappingDto
    {
        public string Category { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;

        public string? RoleId { get; set; }
    }
}