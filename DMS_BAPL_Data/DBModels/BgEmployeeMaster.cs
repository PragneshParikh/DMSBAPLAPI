using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels;

public partial class BgEmployeeMaster
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // ---- Personal ---------------------------------------
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
    public bool IsActive { get; set; } = true;

    public int? Department { get; set; }

    // ---- Login ------------------------------------------
    public string? EmailId { get; set; }

    public string? Password { get; set; }

    public string? MappedZoneIds { get; set; }

    public string? MappedZones { get; set; }

    // ---- Employee mapping cache -------------------------
    public string? MappedEmployeeIds { get; set; }

    public string? MappedEmployees { get; set; }

    // ---- Misc -------------------------------------------
    public string? ProfileImage { get; set; }

    public string? DealerCode { get; set; }

    public string? LocationCode { get; set; }

    // ---- Audit ------------------------------------------
    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? ProfileId { get; set; }

    public virtual ICollection<BgEmployeeProfileMapping> BgEmployeeProfileMappings { get; set; } = new List<BgEmployeeProfileMapping>();

    public virtual City? CityNavigation { get; set; }

    public virtual DepartmentMaster? DepartmentNavigation { get; set; }

    public virtual State? StateNavigation { get; set; }
}
