using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.EmployeeProfileMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Service.Services.EmployeeProfileMasterService
{
    public class EmployeeProfileMasterService : IEmployeeProfileMasterService
    {
        private readonly IEmployeeProfileMasterRepo _repo;

        public EmployeeProfileMasterService(IEmployeeProfileMasterRepo repo)
        {
            _repo = repo;
        }

        // =====================================================
        // GET ALL PROFILES
        // Ordered by SortOrder:
        //   City Head(1) → District Head(2) → Zone Head(3)
        //   → State Head(4) → National Head(5)
        // =====================================================

        public async Task<IEnumerable<EmployeeProfileMaster>> GetAllProfiles()
        {
            try { return await _repo.GetAllProfiles(); }
            catch { throw; }
        }

        // =====================================================
        // GET MAPPINGS BY BG EMPLOYEE
        // Returns all Employee+Profile mappings for one BG emp
        // =====================================================

        public async Task<IEnumerable<BgEmployeeProfileMappingViewModel>> GetMappingsByBgEmployee(int bgEmployeeId)
        {
            try { return await _repo.GetMappingsByBgEmployee(bgEmployeeId); }
            catch { throw; }
        }

        // =====================================================
        // SAVE MAPPINGS
        // Replaces all existing mappings for the BG employee
        // (delete-then-insert)
        // =====================================================

        public async Task<int> SaveMappings(BgEmployeeProfileMappingSaveRequest request)
        {
            try
            {
                var entities = (request.Mappings ?? new())
                    .Select(m => new BgEmployeeProfileMapping
                    {
                        EmployeeId = m.EmployeeId,
                        ProfileId = m.ProfileId,
                    })
                    .ToList();

                return await _repo.SaveMappings(
                    request.BgEmployeeId,
                    entities,
                    request.CreatedBy);
            }
            catch { throw; }
        }
    }
}