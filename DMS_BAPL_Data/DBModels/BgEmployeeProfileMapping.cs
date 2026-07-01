using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels;

public partial class BgEmployeeProfileMapping
{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int BgEmployeeId { get; set; }

    public int EmployeeId { get; set; }

    public int ProfileId { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual BgEmployeeMaster BgEmployee { get; set; } = null!;

    public virtual EmployeeMaster Employee { get; set; } = null!;

    public virtual EmployeeProfileMaster Profile { get; set; } = null!;
}
