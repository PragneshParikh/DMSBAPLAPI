using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.RoleRepo
{
    public class RoleRepo : IRoleRepo
    {
        private readonly BapldmsvadContext _context;

        public RoleRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
        {
            return await _context.AspNetRoles.ToListAsync();
        }

        public async Task AddRoleCategoryMapping(RoleCategoryMapping mapping)
        {
            _context.RoleCategoryMappings.Add(mapping);
            await _context.SaveChangesAsync();
        }

        // FIX: returns ALL mappings, system-generated or not. This method is
        // used internally by RoleService.ResolveOrCreateRoleForItemsAsync to
        // (a) find a previously auto-created role to reuse, and (b) locate
        // the mapping row it just inserted so it can read back its RoleId.
        // Both need to see system-generated rows — filtering them out here
        // means a freshly-inserted mapping can never be found again, which
        // is what caused every fresh resolve to fail with "Could not
        // resolve or create a role for the selected items."
        // The "hide auto-created roles from the Role Master list" behavior
        // belongs ONLY in RoleController.GetMappings (which already applies
        // its own .Where(m => !m.IsSystemGenerated) on the projected data).
        // Do not duplicate that filter here.
        public async Task<List<RoleCategoryMapping>> GetAllMappings()
        {
            return await _context.RoleCategoryMappings
                .OrderBy(m => m.Category)
                .ThenBy(m => m.RoleName)
                .ToListAsync();
        }

        public async Task<bool> UpdateMappingNameAndCategory(int id, string roleName, string? category)
        {
            var mapping = await _context.RoleCategoryMappings.FindAsync(id);
            if (mapping == null) return false;

            mapping.RoleName = roleName;
            mapping.Category = category;

            if (!string.IsNullOrWhiteSpace(mapping.RoleId))
            {
                var role = await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Id == mapping.RoleId);
                if (role != null && !string.Equals(role.Name, roleName, StringComparison.Ordinal))
                {
                    role.Name = roleName;
                    role.NormalizedName = roleName.ToUpperInvariant();
                    role.ConcurrencyStamp = Guid.NewGuid().ToString();
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMapping(int id)
        {
            var mapping = await _context.RoleCategoryMappings.FindAsync(id);
            if (mapping == null) return false;

            _context.RoleCategoryMappings.Remove(mapping);
            await _context.SaveChangesAsync();
            return true;
        }

        private static bool IsMenuAccessCategory(string? category) =>
            !string.IsNullOrWhiteSpace(category) &&
            (category.Equals("Sale", StringComparison.OrdinalIgnoreCase) ||
             category.Equals("Sales", StringComparison.OrdinalIgnoreCase) ||
             category.Equals("Service", StringComparison.OrdinalIgnoreCase));

        // NEW — defensive fix, separate from the bug above. RoleWiseMenuRights
        // is keyed by RoleId only, not by category, so if one AspNetRole ever
        // ends up with more than one RoleCategoryMapping row (e.g. resolved
        // once under "Sale" and once under "Service" for the same items),
        // menu access is genuinely shared between them either way. This picks
        // ANY mapping row for the role that falls in the valid category set,
        // instead of whatever row an unordered query happened to return first.
        private async Task<RoleCategoryMapping?> GetValidCategoryMapping(string roleId)
        {
            var mappings = await _context.RoleCategoryMappings
                .AsNoTracking()
                .Where(m => m.RoleId == roleId)
                .ToListAsync();

            return mappings
                .OrderByDescending(m => m.Id)
                .FirstOrDefault(m => IsMenuAccessCategory(m.Category));
        }

        // ═══════════════════════════════════════════════════════════════
        // MENU ACCESS — SALE / SERVICE ROLES ONLY
        // ═══════════════════════════════════════════════════════════════

        public async Task<RoleMenuAccessResponseViewModel?> GetMenuAccessAsync(string roleId)
        {
            var mapping = await GetValidCategoryMapping(roleId);
            if (mapping == null)
                return null;

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
                .Where(r => r.RoleId == roleId && subMenuIds.Contains(r.SubMenuId))
                .Select(r => r.SubMenuId)
                .ToListAsync())
                .ToHashSet();

            var groups = topMenus.Select(top => new DealerMenuAccessGroupViewModel
            {
                TopMenuId = top.Id,
                TopMenuName = top.MenuName ?? string.Empty,
                Items = subMenus
                    .Where(s => s.ParentMenuId == top.Id)
                    .Select(s => new DealerMenuAccessItemViewModel
                    {
                        SubMenuId = s.Id,
                        MenuName = s.MenuName ?? string.Empty,
                        PathName = s.PathName,
                        IsGranted = grantedSubMenuIds.Contains(s.Id)
                    })
                    .ToList()
            }).ToList();

            return new RoleMenuAccessResponseViewModel
            {
                RoleId = roleId,
                RoleName = mapping.RoleName,
                Category = mapping.Category,
                Groups = groups
            };
        }

        public async Task<(bool Success, string? Error)> UpdateMenuAccessAsync(string roleId, List<int> grantedSubMenuIds, string updatedBy)
        {
            var mapping = await GetValidCategoryMapping(roleId);
            if (mapping == null)
                return (false, "This role is not registered under the Sale or Service category.");

            var roleExists = await _context.AspNetRoles.AnyAsync(r => r.Id == roleId);
            if (!roleExists) return (false, "Selected role no longer exists.");

            var topMenus = await _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId == null && (m.MenuName == "Process" || m.MenuName == "Reports"))
                .ToListAsync();
            var topMenuIds = topMenus.Select(m => m.Id).ToList();

            var validSubMenus = await _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId.HasValue && topMenuIds.Contains(m.ParentMenuId.Value))
                .ToDictionaryAsync(m => m.Id, m => m.ParentMenuId!.Value);

            var requestedIds = grantedSubMenuIds.Where(id => validSubMenus.ContainsKey(id)).ToHashSet();

            var existingRights = await _context.RoleWiseMenuRights
                .Where(r => r.RoleId == roleId && validSubMenus.Keys.Contains(r.SubMenuId))
                .ToListAsync();

            var existingIds = existingRights.Select(r => r.SubMenuId).ToHashSet();

            var toAdd = requestedIds.Except(existingIds).ToList();
            var toRemove = existingRights.Where(r => !requestedIds.Contains(r.SubMenuId)).ToList();

            if (toRemove.Any())
                _context.RoleWiseMenuRights.RemoveRange(toRemove);

            foreach (var subMenuId in toAdd)
            {
                _context.RoleWiseMenuRights.Add(new RoleWiseMenuRight
                {
                    RoleId = roleId,
                    MenuId = validSubMenus[subMenuId],
                    SubMenuId = subMenuId,
                    Permission = 4,
                    CreatedBy = updatedBy,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<DealerMenuAccessGroupViewModel>> GetMenuTemplateAsync()
        {
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

            return topMenus.Select(top => new DealerMenuAccessGroupViewModel
            {
                TopMenuId = top.Id,
                TopMenuName = top.MenuName ?? string.Empty,
                Items = subMenus
                    .Where(s => s.ParentMenuId == top.Id)
                    .Select(s => new DealerMenuAccessItemViewModel
                    {
                        SubMenuId = s.Id,
                        MenuName = s.MenuName ?? string.Empty,
                        PathName = s.PathName,
                        IsGranted = false
                    })
                    .ToList()
            }).ToList();
        }
    }
}