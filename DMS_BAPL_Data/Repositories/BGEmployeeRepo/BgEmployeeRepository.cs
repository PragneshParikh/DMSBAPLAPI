using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo
{
    public partial class BgEmployeeMasterRepo : IBgEmployeeMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public BgEmployeeMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL
        // =====================================================

        async Task<IEnumerable<BgEmployeeMaster>> IBgEmployeeMasterRepo.Get()
        {
            try
            {
                return await Task.FromResult(_context.BgEmployeeMasters.ToList());
            }
            catch { throw; }
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        async Task<BgEmployeeMaster?> IBgEmployeeMasterRepo.GetById(int id)
        {
            try
            {
                return await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        // =====================================================
        // CREATE — plain insert, no duplicate-check here
        // =====================================================

        async Task<BgEmployeeMaster> IBgEmployeeMasterRepo.Create(BgEmployeeMaster bgEmployee)
        {
            try
            {
                bgEmployee.CreatedDate = DateTime.Now;
                bgEmployee.UpdatedDate = DateTime.Now;

                _context.BgEmployeeMasters.Add(bgEmployee);
                await _context.SaveChangesAsync();

                return bgEmployee;
            }
            catch { throw; }
        }

        // =====================================================
        // UPDATE
        // =====================================================

        async Task<int> IBgEmployeeMasterRepo.Update(BgEmployeeMaster bgEmployee)
        {
            try
            {
                var existing = await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == bgEmployee.Id);

                if (existing == null)
                    return 0;

                existing.FirstName = bgEmployee.FirstName;
                existing.LastName = bgEmployee.LastName;
                existing.Gender = bgEmployee.Gender;
                existing.Mobile = bgEmployee.Mobile;
                existing.State = bgEmployee.State;
                existing.City = bgEmployee.City;
                existing.Pincode = bgEmployee.Pincode;
                existing.DateOfBirth = bgEmployee.DateOfBirth;
                existing.DateOfJoin = bgEmployee.DateOfJoin;
                existing.EffectiveDate = bgEmployee.EffectiveDate;
                existing.ReportingTo = bgEmployee.ReportingTo;
                existing.IsActive = bgEmployee.IsActive;
                existing.Department = bgEmployee.Department;
                existing.ProfileId = bgEmployee.ProfileId;
                existing.EmailId = bgEmployee.EmailId;
                existing.MappedZones = bgEmployee.MappedZones;
                existing.MappedZoneIds = bgEmployee.MappedZoneIds;
                existing.ProfileImage = bgEmployee.ProfileImage;
                existing.DealerCode = bgEmployee.DealerCode;
                existing.LocationCode = bgEmployee.LocationCode;
                existing.UpdatedBy = "admin";
                existing.UpdatedDate = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(bgEmployee.Password))
                    existing.Password = bgEmployee.Password;

                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        // =====================================================
        // UPDATE STATUS ONLY — NEW.
        // Deliberately touches nothing but IsActive/UpdatedBy/UpdatedDate.
        // Used by the list page's status toggle, which never has (and
        // shouldn't need) the full employee record on hand.
        // =====================================================

        async Task<int> IBgEmployeeMasterRepo.UpdateStatus(int id, bool isActive)
        {
            try
            {
                var existing = await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existing == null)
                    return 0;

                existing.IsActive = isActive;
                existing.UpdatedBy = "admin";
                existing.UpdatedDate = DateTime.Now;

                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        // =====================================================
        // DELETE
        // =====================================================

        async Task<int> IBgEmployeeMasterRepo.Delete(int id)
        {
            try
            {
                var existing = await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existing == null)
                    return 0;

                _context.BgEmployeeMasters.Remove(existing);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        // =====================================================
        // GET BY EMAIL
        // =====================================================

        async Task<BgEmployeeMaster?> IBgEmployeeMasterRepo.GetByEmail(string email)
        {
            try
            {
                var normalizedEmail = email?.Trim().ToLowerInvariant();

                return await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.EmailId.ToLower() == normalizedEmail);
            }
            catch { throw; }
        }

        async Task<IEnumerable<AssignedDealerInfo>> IBgEmployeeMasterRepo.GetAssignedDealerCodes(int excludeEmployeeId)
        {
            var rows = await _context.BgEmployeeMasters
                .Where(e => e.IsActive
                         && !string.IsNullOrWhiteSpace(e.DealerCode)
                         && e.Id != excludeEmployeeId)
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.DealerCode
                })
                .ToListAsync();

            return rows.SelectMany(e =>
                (e.DealerCode ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(code => new AssignedDealerInfo(
                        e.Id,
                        e.EmployeeCode,
                        $"{e.FirstName} {e.LastName}".Trim(),
                        code.Trim()
                    ))
            );
        }

        async Task<int> IBgEmployeeMasterRepo.UpdateEntity(BgEmployeeMaster bgEmployee)
        {
            try
            {
                var existing = await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == bgEmployee.Id);

                if (existing == null) return 0;

                existing.DealerCode = bgEmployee.DealerCode;
                existing.MappedZones = bgEmployee.MappedZones;
                existing.MappedZoneIds = bgEmployee.MappedZoneIds;
                existing.UpdatedBy = bgEmployee.UpdatedBy;
                existing.UpdatedDate = bgEmployee.UpdatedDate;

                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        async Task IBgEmployeeMasterRepo.SaveRoleMappings(int employeeId, List<RoleMappingDto> roleMappings)
        {
            try
            {
                var existing = _context.BgEmployeeRoleMappings
                    .Where(m => m.BgEmployeeId == employeeId);

                _context.BgEmployeeRoleMappings.RemoveRange(existing);

                if (roleMappings?.Any() == true)
                {
                    var newRows = roleMappings
                        .Where(m => !string.IsNullOrWhiteSpace(m.Category) && !string.IsNullOrWhiteSpace(m.RoleName))
                        .Select(m => new BgEmployeeRoleMapping
                        {
                            BgEmployeeId = employeeId,
                            Category = m.Category.Trim(),
                            RoleName = m.RoleName.Trim(),
                            CreatedDate = DateTime.Now,
                            CreatedBy = "system"
                        });

                    await _context.BgEmployeeRoleMappings.AddRangeAsync(newRows);
                }

                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        async Task<IEnumerable<BgEmployeeRoleMapping>> IBgEmployeeMasterRepo.GetRoleMappings(int employeeId)
        {
            try
            {
                return await _context.BgEmployeeRoleMappings
                    .Where(m => m.BgEmployeeId == employeeId)
                    .ToListAsync();
            }
            catch { throw; }
        }

        async Task<IEnumerable<BgEmployeeListItemViewModel>> IBgEmployeeMasterRepo.GetEmployeeListView()
        {
            try
            {
                var employees = await _context.BgEmployeeMasters.ToListAsync();

                var dealers = await _context.DealerMasters
                    .Select(d => new { d.Dealercode, d.Compname })
                    .ToListAsync();

                var dealerMap = dealers
                    .Where(d => !string.IsNullOrWhiteSpace(d.Dealercode))
                    .GroupBy(d => d.Dealercode.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Compname, StringComparer.OrdinalIgnoreCase);

                var roleMappings = await _context.BgEmployeeRoleMappings.ToListAsync();
                var roleMap = roleMappings
                    .GroupBy(r => r.BgEmployeeId)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.RoleName).Distinct()));

                var employeeNameMap = employees
                    .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

                var result = employees.Select(e =>
                {
                    var dealerCodes = (e.DealerCode ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();

                    var dealerNames = dealerCodes
                        .Select(c => dealerMap.TryGetValue(c, out var name) ? name : c)
                        .Distinct();

                    string? reportingToName = null;
                    if (!string.IsNullOrWhiteSpace(e.ReportingTo))
                    {
                        reportingToName = int.TryParse(e.ReportingTo, out var reportId)
                                           && employeeNameMap.TryGetValue(reportId, out var name)
                            ? name
                            : e.ReportingTo;
                    }

                    return new BgEmployeeListItemViewModel
                    {
                        Id = e.Id,
                        EmployeeCode = e.EmployeeCode,
                        EmployeeName = $"{e.FirstName} {e.LastName}".Trim(),
                        DealerCode = e.DealerCode,
                        DealerName = string.Join(", ", dealerNames),
                        Zone = e.MappedZones,
                        JobRoles = roleMap.TryGetValue(e.Id, out var roles) ? roles : "",
                        CreatedDate = e.CreatedDate,
                        UpdatedDate = e.UpdatedDate,
                        ReportingTo = reportingToName,
                        CreatedBy = e.CreatedBy,
                        IsActive = e.IsActive,
                        ProfileImage = e.ProfileImage,
                    };
                });

                return result;
            }
            catch { throw; }
        }
    }
}