using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.EmployeeProfileMasterRepo
{
    public interface IEmployeeProfileMasterRepo
    {
        // Profile Master (read-only — data is seeded)
        Task<IEnumerable<EmployeeProfileMaster>> GetAllProfiles();

        // Profile Mappings per BG Employee
        Task<IEnumerable<BgEmployeeProfileMappingViewModel>> GetMappingsByBgEmployee(int bgEmployeeId);
        Task<int> SaveMappings(int bgEmployeeId, List<BgEmployeeProfileMapping> mappings, string? createdBy);
    }
}
