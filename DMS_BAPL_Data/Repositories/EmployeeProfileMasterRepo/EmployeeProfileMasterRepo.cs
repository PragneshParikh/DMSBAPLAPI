using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.EmployeeProfileMasterRepo
{
    public partial class EmployeeProfileMasterRepo : IEmployeeProfileMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public EmployeeProfileMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL PROFILES (lookup — Id + ProfileName only)
        // =====================================================

        async Task<IEnumerable<EmployeeProfileMaster>> IEmployeeProfileMasterRepo.GetAllProfiles()
        {
            try
            {
                return await Task.FromResult(
                    _context.EmployeeProfileMasters
                        .OrderBy(p => p.SortOrder)
                        .ToList()
                );
            }
            catch { throw; }
        }

        // =====================================================
        // GET MAPPINGS BY BG EMPLOYEE
        // =====================================================

        async Task<IEnumerable<BgEmployeeProfileMappingViewModel>>IEmployeeProfileMasterRepo.GetMappingsByBgEmployee(int bgEmployeeId)
        {
            try
            {
                var mappings = await _context.BgEmployeeProfileMappings
                    .Where(m => m.BgEmployeeId == bgEmployeeId && m.IsActive)
                    .ToListAsync();

                var employeeIds = mappings.Select(m => m.EmployeeId).Distinct().ToList();
                var profileIds = mappings.Select(m => m.ProfileId).Distinct().ToList();

                var employees = await _context.EmployeeMasters
                    .Where(e => employeeIds.Contains(e.Id))
                    .ToDictionaryAsync(e => e.Id,
                        e => new { e.FirstName, e.LastName, e.EmployeeCode });

                var profiles = await _context.EmployeeProfileMasters
                    .Where(p => profileIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.ProfileName);

                return mappings.Select(m => new BgEmployeeProfileMappingViewModel
                {
                    Id = m.Id,
                    BgEmployeeId = m.BgEmployeeId,
                    EmployeeId = m.EmployeeId,
                    ProfileId = m.ProfileId,
                    IsActive = m.IsActive,
                    CreatedDate = m.CreatedDate,
                    EmployeeName = employees.ContainsKey(m.EmployeeId)
                                   ? $"{employees[m.EmployeeId].FirstName} {employees[m.EmployeeId].LastName}".Trim()
                                   : string.Empty,
                    EmployeeCode = employees.ContainsKey(m.EmployeeId)
                                   ? employees[m.EmployeeId].EmployeeCode
                                   : string.Empty,
                    ProfileName = profiles.ContainsKey(m.ProfileId)
                                   ? profiles[m.ProfileId]
                                   : string.Empty,
                });
            }
            catch { throw; }
        }

        // =====================================================
        // SAVE MAPPINGS (delete-then-insert per BG employee)
        // Matches the SaveEmployeeRoleMappings pattern exactly
        // =====================================================

        async Task<int> IEmployeeProfileMasterRepo.SaveMappings(int bgEmployeeId,List<BgEmployeeProfileMapping> mappings,string? createdBy)
        {
            try
            {
                // remove all existing mappings for this BG employee
                var old = _context.BgEmployeeProfileMappings
                    .Where(m => m.BgEmployeeId == bgEmployeeId);
                _context.BgEmployeeProfileMappings.RemoveRange(old);

                // insert the new set
                foreach (var m in mappings)
                {
                    m.BgEmployeeId = bgEmployeeId;
                    m.IsActive = true;
                    m.CreatedBy = createdBy ?? "admin";
                    m.CreatedDate = DateTime.Now;
                    m.UpdatedBy = createdBy ?? "admin";
                    m.UpdatedDate = DateTime.Now;
                    _context.BgEmployeeProfileMappings.Add(m);
                }

                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }
    }
}
