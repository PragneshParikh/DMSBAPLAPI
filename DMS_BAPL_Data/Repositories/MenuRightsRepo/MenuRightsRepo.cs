using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.MenuRightsRepo
{
    public class MenuRightsRepo : IMenuRightsRepo
    {
        private readonly BapldmsvadContext _context;

        public MenuRightsRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<DealerDropdownViewModel>> GetDealersAsync()
        {
            var dealers = await _context.DealerMasters
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.Compname)
                .Select(d => new { d.Id, d.Dealercode, d.Compname })
                .ToListAsync();

            // Pull ALL dealer-linked users + their non-Employee role in one
            // raw SQL round trip instead of one query per dealer — avoids the
            // Dictionary<string,object> DbSet issue entirely, since raw SQL
            // reads rows directly without needing an EF-mapped entity type.
            var userRoleRows = await _context.Database
                .SqlQuery<UserRoleRow>($@"
                    SELECT u.DealerCode AS DealerCode, u.Id AS UserId, r.Id AS RoleId, r.Name AS RoleName
                    FROM AspNetUsers u
                    INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                    INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                    WHERE u.DealerCode IS NOT NULL
                      AND r.Name <> 'Employee'
                ")
                .ToListAsync();

            var userRoleLookup = userRoleRows
                .GroupBy(x => x.DealerCode)
                .ToDictionary(g => g.Key, g => g.First());

            var result = dealers.Select(d =>
            {
                userRoleLookup.TryGetValue(d.Dealercode, out var match);

                return new DealerDropdownViewModel
                {
                    Id = d.Id,
                    DealerCode = d.Dealercode,
                    DealerName = d.Compname,
                    CompName = d.Compname,
                    UserId = match?.UserId ?? "",
                    RoleId = match?.RoleId,
                    RoleName = match?.RoleName
                };
            }).ToList();

            return result;
        }

        public async Task<(string? UserId, string? RoleId, string? RoleName)> GetDealerRoleAsync(string dealerCode)
        {
            var row = await _context.Database
                .SqlQuery<UserRoleRow>($@"
                    SELECT u.DealerCode AS DealerCode, u.Id AS UserId, r.Id AS RoleId, r.Name AS RoleName
                    FROM AspNetUsers u
                    INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                    INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                    WHERE u.DealerCode = {dealerCode}
                      AND r.Name <> 'Employee'
                ")
                .FirstOrDefaultAsync();

            return (row?.UserId, row?.RoleId, row?.RoleName);
        }

        public async Task<List<MenuGroupViewModel>> GetMenuTreeWithRightsAsync(string? roleId)
        {
            var allMenus = await _context.MenuMasters
                .OrderBy(m => m.SerialNo)
                .ToListAsync();

            var existingRights = string.IsNullOrEmpty(roleId)
                ? new List<RoleWiseMenuRight>()
                : await _context.RoleWiseMenuRights
                    .Where(r => r.RoleId == roleId)
                    .ToListAsync();

            var rightsLookup = existingRights
                .GroupBy(r => (r.MenuId, r.SubMenuId))
                .ToDictionary(g => g.Key, g => g.First().Permission);

            var parents = allMenus.Where(m => m.ParentMenuId == null).ToList();

            var result = new List<MenuGroupViewModel>();

            foreach (var parent in parents)
            {
                var children = allMenus.Where(m => m.ParentMenuId == parent.Id).OrderBy(m => m.SerialNo).ToList();

                var group = new MenuGroupViewModel
                {
                    MenuId = parent.Id,
                    MenuName = parent.MenuName,
                    ModuleName = parent.ModuleName
                };

                if (children.Any())
                {
                    foreach (var child in children)
                    {
                        rightsLookup.TryGetValue((parent.Id, child.Id), out var permission);

                        group.SubMenus.Add(new SubMenuRightViewModel
                        {
                            SubMenuId = child.Id,
                            MenuName = child.MenuName,
                            PathName = child.PathName,
                            Permission = permission
                        });
                    }
                }
                else
                {
                    rightsLookup.TryGetValue((parent.Id, parent.Id), out var permission);

                    group.SubMenus.Add(new SubMenuRightViewModel
                    {
                        SubMenuId = parent.Id,
                        MenuName = parent.MenuName,
                        PathName = parent.PathName,
                        Permission = permission
                    });
                }

                result.Add(group);
            }

            return result;
        }

        public async Task SaveMenuRightsAsync(string roleId, List<MenuRightItem> rights, string updatedBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.RoleWiseMenuRights
                    .Where(r => r.RoleId == roleId)
                    .ToListAsync();

                var existingLookup = existing.ToDictionary(r => (r.MenuId, r.SubMenuId), r => r);

                foreach (var item in rights)
                {
                    if (existingLookup.TryGetValue((item.MenuId, item.SubMenuId), out var existingRow))
                    {
                        existingRow.Permission = item.Permission;
                        existingRow.UpdatedBy = updatedBy;
                        existingRow.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        _context.RoleWiseMenuRights.Add(new RoleWiseMenuRight
                        {
                            RoleId = roleId,
                            MenuId = item.MenuId,
                            SubMenuId = item.SubMenuId,
                            Permission = item.Permission,
                            CreatedBy = updatedBy,
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Helper DTO for raw SQL projection — not an EF entity, just a
        // plain shape for SqlQuery<T> to materialize rows into.
        private class UserRoleRow
        {
            public string DealerCode { get; set; } = null!;
            public string UserId { get; set; } = null!;
            public string RoleId { get; set; } = null!;
            public string RoleName { get; set; } = null!;
        }
    }
}