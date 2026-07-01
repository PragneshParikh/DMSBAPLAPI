using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels;

public partial class EmployeeProfileMaster
{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string ProfileName { get; set; } = null!;

    public int SortOrder { get; set; }

    public virtual ICollection<BgEmployeeProfileMapping> BgEmployeeProfileMappings { get; set; } = new List<BgEmployeeProfileMapping>();
}
