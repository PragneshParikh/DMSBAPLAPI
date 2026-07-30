using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.DBModels
{
    public partial class EmployeeMaster
    {
        // NOT a real column — carries the checked Category/Role pairs from
        // the Angular payload through to EmployeeMasterRepo.CreateNewUser /
        // UpdateEmployee, which persist them into EmployeeRoleMapping rows.
        // [NotMapped] is required: without it, EF Core tries to map this to
        // a non-existent database column and throws when the model builds.
        [NotMapped]
        public List<RoleMappingDto> RoleMappings { get; set; } = new();
    }
}