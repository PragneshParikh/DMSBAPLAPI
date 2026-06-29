using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Service.Services.EmployeeProfileMasterService
{
    public interface IEmployeeProfileMasterService
    {
        // Profile Master — read-only (seeded in DB)
        Task<IEnumerable<EmployeeProfileMaster>> GetAllProfiles();

        // Profile Mappings per BG Employee
        Task<IEnumerable<BgEmployeeProfileMappingViewModel>> GetMappingsByBgEmployee(int bgEmployeeId);
        Task<int> SaveMappings(BgEmployeeProfileMappingSaveRequest request);
    }
}