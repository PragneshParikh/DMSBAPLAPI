using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class EmployeeMaster
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Gender { get; set; }

    public string? Mobile { get; set; }

    public string? EmailId { get; set; }

    public string? Password { get; set; }

    public string? Address { get; set; }

    public int? State { get; set; }

    public int? City { get; set; }

    public int? Pincode { get; set; }

    public DateTime DateOfJoin { get; set; }

    public string? Designation { get; set; }

    public string? Department { get; set; }

    public string DealerCode { get; set; } = null!;

    public string? Supervisor { get; set; }

    public bool IsActive { get; set; }

    public string? ProfileImage { get; set; }

    public string? Notes { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}