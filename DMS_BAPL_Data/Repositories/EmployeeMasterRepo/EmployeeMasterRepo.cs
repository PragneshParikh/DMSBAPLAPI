using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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

        public EmployeeMasterRepo(BapldmsvadContext context)
        {
            _context = context;
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