using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.EmployeeMasterRepo
{
    public partial class EmployeeMasterRepo : IEmployeeMasterRepo
    {
        private readonly BapldmsvadContext _context;

        private readonly IDataProtector _locationPasswordProtector;

        public EmployeeMasterRepo(BapldmsvadContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _locationPasswordProtector = dataProtectionProvider.CreateProtector("LocationPassword.v1");
        }

        async Task<IEnumerable<EmployeeMaster>> IEmployeeMasterRepo.Get()
        {
            try
            {
                return await Task.FromResult(_context.EmployeeMasters.ToList());
            }
            catch { throw; }
        }

        async Task<EmployeeMaster?> IEmployeeMasterRepo.GetEmployeeById(int id)
        {
            try
            {
                return await _context.EmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        async Task<int> IEmployeeMasterRepo.CreateNewUser(EmployeeMaster employeeMaster)
        {
            try
            {
                // NEW — location login: enforce a unique Location Login ID and
                // turn whatever plaintext password came in from the client into
                // an encrypted value before it ever reaches the database.
                if (!string.IsNullOrWhiteSpace(employeeMaster.LocationLoginId))
                {
                    await EnsureLocationLoginIdIsUnique(employeeMaster.LocationLoginId, excludeEmployeeId: 0);
                }
                ProtectLocationPasswordIfProvided(employeeMaster, existingProtectedValue: null);

                // FIXED: force new employees active by default. See header
                // comment — AuthController.Login rejects any Employee-role
                // login with 401 whenever this is false, and it defaults to
                // false in C# unless explicitly set. Creating a brand-new
                // employee who is immediately inactive isn't a realistic
                // scenario; deactivate via Update afterward if truly needed.
                employeeMaster.IsActive = true;

                _context.EmployeeMasters.Add(employeeMaster);
                var result = await _context.SaveChangesAsync();

                await SaveEmployeeRoleMappings(employeeMaster.Id, employeeMaster.RoleMappings);

                return result;
            }
            catch { throw; }
        }

        async Task<int> IEmployeeMasterRepo.UpdateEmployee(EmployeeMaster employeeMaster)
        {
            try
            {
                var existingEmployee = await _context.EmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == employeeMaster.Id);

                if (existingEmployee == null)
                    return 0;

                // NEW — same uniqueness + encryption rules as create, excluding
                // this employee's own row from the uniqueness check, and
                // falling back to the existing encrypted value whenever the
                // password field was left blank (so editing an employee
                // doesn't force re-entering the password).
                if (!string.IsNullOrWhiteSpace(employeeMaster.LocationLoginId))
                {
                    await EnsureLocationLoginIdIsUnique(employeeMaster.LocationLoginId, excludeEmployeeId: existingEmployee.Id);
                }
                ProtectLocationPasswordIfProvided(employeeMaster, existingEmployee.LocationPasswordHash);

                existingEmployee.EmployeeCode = employeeMaster.EmployeeCode;
                existingEmployee.FirstName = employeeMaster.FirstName;
                existingEmployee.LastName = employeeMaster.LastName;
                existingEmployee.Gender = employeeMaster.Gender;
                existingEmployee.Mobile = employeeMaster.Mobile;
                existingEmployee.EmailId = employeeMaster.EmailId;
                existingEmployee.Password = employeeMaster.Password;
                existingEmployee.Address = employeeMaster.Address;
                existingEmployee.State = employeeMaster.State;
                existingEmployee.City = employeeMaster.City;
                existingEmployee.Pincode = employeeMaster.Pincode;
                existingEmployee.DateOfJoin = employeeMaster.DateOfJoin;
                existingEmployee.Designation = employeeMaster.Designation;
                existingEmployee.Department = employeeMaster.Department;
                existingEmployee.DealerCode = employeeMaster.DealerCode;
                existingEmployee.LocationCode = employeeMaster.LocationCode;
                existingEmployee.LocationLoginId = employeeMaster.LocationLoginId;           // NEW
                existingEmployee.LocationPasswordHash = employeeMaster.LocationPasswordHash; // NEW
                existingEmployee.Supervisor = employeeMaster.Supervisor;
                existingEmployee.IsActive = employeeMaster.IsActive;
                existingEmployee.ProfileImage = employeeMaster.ProfileImage;
                existingEmployee.Notes = employeeMaster.Notes;
                existingEmployee.UpdatedBy = "admin";
                existingEmployee.UpdatedDate = DateTime.Now;

                var result = await _context.SaveChangesAsync();

                await SaveEmployeeRoleMappings(existingEmployee.Id, employeeMaster.RoleMappings);

                return result;
            }
            catch { throw; }
        }

        // NEW — Location Login ID has to be unique across employees since it's
        // the lookup key the location-login authentication endpoint uses.
        private async Task EnsureLocationLoginIdIsUnique(string locationLoginId, int excludeEmployeeId)
        {
            var normalized = locationLoginId.Trim();

            var exists = await _context.EmployeeMasters
                .AnyAsync(x => x.Id != excludeEmployeeId &&
                               x.LocationLoginId != null &&
                               x.LocationLoginId.ToLower() == normalized.ToLower());

            if (exists)
                throw new InvalidOperationException($"Location Login ID '{locationLoginId}' is already in use by another employee.");
        }

        // CHANGED — LocationPasswordHash arrives from the client holding a
        // *plaintext* password whenever the admin typed a new one into the
        // "Location Password" field. Blank means "leave the stored value
        // untouched" on update. No LocationLoginId at all means location
        // login isn't configured for this employee, so the stored value is
        // cleared. The plaintext is now encrypted (reversibly) rather than
        // one-way hashed, so it can be decrypted and shown back later.
        private void ProtectLocationPasswordIfProvided(EmployeeMaster employeeMaster, string? existingProtectedValue)
        {
            if (string.IsNullOrWhiteSpace(employeeMaster.LocationLoginId))
            {
                employeeMaster.LocationPasswordHash = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(employeeMaster.LocationPasswordHash))
            {
                employeeMaster.LocationPasswordHash = existingProtectedValue;
                return;
            }

            employeeMaster.LocationPasswordHash =
                _locationPasswordProtector.Protect(employeeMaster.LocationPasswordHash);
        }

        private async Task SaveEmployeeRoleMappings(int employeeId, List<RoleMappingDto>? roleMappings)
        {
            var old = _context.EmployeeRoleMappings
                .Where(m => m.EmployeeId == employeeId);
            _context.EmployeeRoleMappings.RemoveRange(old);

            if (roleMappings?.Any() == true)
            {
                var newRows = roleMappings
                    .Where(m => !string.IsNullOrWhiteSpace(m.Category) && !string.IsNullOrWhiteSpace(m.RoleName))
                    .Select(m => new EmployeeRoleMapping
                    {
                        EmployeeId = employeeId,
                        Category = m.Category.Trim(),
                        RoleName = m.RoleName.Trim(),
                        RoleId = m.RoleId,
                        CreatedDate = DateTime.Now
                    });

                await _context.EmployeeRoleMappings.AddRangeAsync(newRows);
            }

            await _context.SaveChangesAsync();
        }

        async Task<IEnumerable<EmployeeRoleMapping>> IEmployeeMasterRepo.GetRoleMappings(int employeeId)
        {
            try
            {
                return await _context.EmployeeRoleMappings
                    .Where(m => m.EmployeeId == employeeId)
                    .ToListAsync();
            }
            catch { throw; }
        }

        async Task<List<EmployeeDesignationWiseViewModel>> IEmployeeMasterRepo.GetEmployeesByDesignation(string? dealerCode, string designation)
        {
            try
            {
                var result = await _context.EmployeeMasters
                    .Where(e => e.Designation.ToLower() == designation.ToLower() && e.IsActive)
                    .Select(e => new EmployeeDesignationWiseViewModel
                    {
                        EmployeeCode = e.EmployeeCode,
                        Designation = e.Designation,
                        Department = e.Department,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        DealerCode = e.DealerCode,
                        LocationCode = e.LocationCode,
                    })
                    .ToListAsync();
                if (!string.IsNullOrEmpty(dealerCode))
                {
                    result = result.Where(i => i.DealerCode == dealerCode).ToList();
                }
                return result;
            }
            catch { throw; }
        }

        async Task<EmployeeMaster?> IEmployeeMasterRepo.GetEmployeeByEmail(string email)
        {
            try
            {
                var normalizedEmail = email?.Trim().ToLowerInvariant();

                return await _context.EmployeeMasters
                    .FirstOrDefaultAsync(x => x.EmailId.ToLower() == normalizedEmail);
            }
            catch { throw; }
        }

        // NEW — lookup used by the location-login authentication endpoint.
        async Task<EmployeeMaster?> IEmployeeMasterRepo.GetEmployeeByLocationLoginId(string locationLoginId)
        {
            try
            {
                var normalized = locationLoginId?.Trim().ToLowerInvariant();

                return await _context.EmployeeMasters
                    .FirstOrDefaultAsync(x => x.LocationLoginId != null &&
                                              x.LocationLoginId.ToLower() == normalized);
            }
            catch { throw; }
        }

        async Task<List<EmployeeMenuGroupViewModel>> IEmployeeMasterRepo.GetMenuForRolesAsync(List<string> roleIds)
        {
            if (roleIds == null || roleIds.Count == 0)
                return new List<EmployeeMenuGroupViewModel>();

            var topMenus = await _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId == null && (m.MenuName == "Process" || m.MenuName == "Reports"))
                .ToListAsync();

            var topMenuIds = topMenus.Select(m => m.Id).ToList();

            var subMenus = await _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId.HasValue && topMenuIds.Contains(m.ParentMenuId.Value))
                .OrderBy(m => m.SerialNo)
                .ToListAsync();

            var subMenuIds = subMenus.Select(s => s.Id).ToList();

            var grantedSubMenuIds = (await _context.RoleWiseMenuRights
                .AsNoTracking()
                .Where(r => roleIds.Contains(r.RoleId) && subMenuIds.Contains(r.SubMenuId))
                .Select(r => r.SubMenuId)
                .Distinct()
                .ToListAsync())
                .ToHashSet();

            return topMenus
                .Select(top => new EmployeeMenuGroupViewModel
                {
                    TopMenuId = top.Id,
                    TopMenuName = top.MenuName ?? string.Empty,
                    Items = subMenus
                        .Where(s => s.ParentMenuId == top.Id && grantedSubMenuIds.Contains(s.Id))
                        .Select(s => new EmployeeMenuItemViewModel
                        {
                            SubMenuId = s.Id,
                            MenuName = s.MenuName ?? string.Empty,
                            PathName = s.PathName
                        })
                        .ToList()
                })
                .Where(g => g.Items.Count > 0)
                .ToList();
        }
    }
}