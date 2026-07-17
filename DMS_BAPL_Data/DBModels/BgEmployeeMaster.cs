using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class BgEmployeeMaster
{
    public int Id { get; set; }

    public string? EmployeeCode { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

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

    public string? EmailId { get; set; }

    public string? Password { get; set; }

    public string? MappedZoneIds { get; set; }

    public string? MappedZones { get; set; }

    public string? MappedEmployeeIds { get; set; }

    public string? MappedEmployees { get; set; }

    public string? ProfileImage { get; set; }

    public string? DealerCode { get; set; }

    public string? LocationCode { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? ProfileId { get; set; }

    public string? Email { get; set; }

    public string? TsmCode { get; set; }

    public string? AreaOfficeId { get; set; }

    public virtual ICollection<BgEmployeeProfileMapping> BgEmployeeProfileMappings { get; set; } = new List<BgEmployeeProfileMapping>();

    public virtual ICollection<BgEmployeeRoleMapping> BgEmployeeRoleMappings { get; set; } = new List<BgEmployeeRoleMapping>();

    public virtual City? CityNavigation { get; set; }

    public virtual DepartmentMaster? DepartmentNavigation { get; set; }

    public virtual State? StateNavigation { get; set; }
}
