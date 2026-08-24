using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.BgRoleRepo
{
    public class BgRoleRepo : IBgRoleRepo
    {
        private readonly BapldmsvadContext _context;

        public BgRoleRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
        {
            return await _context.AspNetRoles.ToListAsync();
        }

        public async Task<AspNetRole?> GetRoleById(string id)
        {
            return await _context.AspNetRoles.FindAsync(id);
        }

        public async Task AddRoleCategoryMapping(BgRoleCategoryMapping mapping)
        {
            _context.BgRoleCategoryMappings.Add(mapping);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BgRoleCategoryMapping>> GetMappingsByCategory(string category)
        {
            return await _context.BgRoleCategoryMappings
                .Where(m => m.Category == category && m.LocationId == null)
                .ToListAsync();
        }

        public async Task<List<BgRoleCategoryMapping>> GetAllMappings()
        {
            return await _context.BgRoleCategoryMappings
                .Where(m => m.LocationId == null)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.RoleName)
                .ToListAsync();
        }

        public async Task<BgRoleCategoryMapping?> GetMappingById(int id)
        {
            return await _context.BgRoleCategoryMappings.FindAsync(id);
        }

        public async Task<bool> UpdateMappingNameAndCategory(int id, string roleName, string? category)
        {
            var mapping = await _context.BgRoleCategoryMappings.FindAsync(id);
            if (mapping == null) return false;

            mapping.RoleName = roleName;
            mapping.Category = category;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMapping(int id)
        {
            var mapping = await _context.BgRoleCategoryMappings.FindAsync(id);
            if (mapping == null) return false;

            _context.BgRoleCategoryMappings.Remove(mapping);
            await _context.SaveChangesAsync();
            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // LOCATION'S OWN ROLE ASSIGNMENT + MENU ACCESS
        // ═══════════════════════════════════════════════════════════════

        public async Task<LocationRoleDetailViewModel?> GetLocationDetailAsync(int locationId)
        {
            var loc = await _context.LocationMasters.AsNoTracking().FirstOrDefaultAsync(l => l.Id == locationId);
            if (loc == null) return null;

            var mapping = await _context.BgRoleCategoryMappings
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.LocationId == locationId);

            return new LocationRoleDetailViewModel
            {
                LocationId = loc.Id,
                LocCode = loc.Loccode,
                LocName = loc.Locname,
                RoleId = mapping?.RoleId,
                RoleName = mapping?.RoleName
            };
        }

        public async Task<(bool Success, string? Error)> UpdateLocationDetailAsync(int locationId, UpdateLocationRoleDetailViewModel model, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(model.LocCode) || string.IsNullOrWhiteSpace(model.LocName))
                return (false, "Location Code and Location Name are required.");

            var loc = await _context.LocationMasters.FirstOrDefaultAsync(l => l.Id == locationId);
            if (loc == null) return (false, "Location not found.");

            loc.Loccode = model.LocCode.Trim();
            loc.Locname = model.LocName.Trim();
            loc.UpdatedBy = updatedBy;
            loc.UpdatedDate = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(model.RoleId))
            {
                var registered = await _context.BgRoleCategoryMappings
                    .FirstOrDefaultAsync(m => m.RoleId == model.RoleId && m.LocationId == null);

                if (registered == null)
                    return (false, "Selected role is not registered in BG Role Master.");

                var locMapping = await _context.BgRoleCategoryMappings
                    .FirstOrDefaultAsync(m => m.LocationId == locationId);

                if (locMapping == null)
                {
                    _context.BgRoleCategoryMappings.Add(new BgRoleCategoryMapping
                    {
                        LocationId = locationId,
                        RoleId = registered.RoleId,
                        RoleName = registered.RoleName,
                        Category = registered.Category,
                        CreatedBy = updatedBy,
                        CreatedDate = DateTime.Now
                    });
                }
                else
                {
                    locMapping.RoleId = registered.RoleId;
                    locMapping.RoleName = registered.RoleName;
                    locMapping.Category = registered.Category;
                }
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // CHANGED — added `module` and `area`, and removed the hardcoded
        // restriction to only "Process"/"Reports" top-level categories.
        // Mirrors DealerManagerRepo.GetMenuAccessAsync exactly: `module`
        // narrows to one top-level MenuMaster category (Master, Process,
        // Reports, Services, Accounts, Utility, Warranty Claim, Stocks,
        // EBW Process, BG Warranty, ...); `area` further narrows to one
        // business area (ShowRoom/WorkShop/Account) via ModuleName.
        public async Task<(string? RoleId, string? RoleName, List<DealerMenuAccessGroupViewModel> Groups)?> GetLocationMenuAccessAsync(int locationId, string? roleId, string? module, string? area)
        {
            var locExists = await _context.LocationMasters.AnyAsync(l => l.Id == locationId);
            if (!locExists) return null;

            string? effectiveRoleId = roleId;
            string? effectiveRoleName = null;

            if (!string.IsNullOrWhiteSpace(effectiveRoleId))
            {
                var role = await _context.AspNetRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == effectiveRoleId);
                effectiveRoleName = role?.Name;
                if (role == null) effectiveRoleId = null;
            }
            else
            {
                var mapping = await _context.BgRoleCategoryMappings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.LocationId == locationId);
                effectiveRoleId = mapping?.RoleId;
                effectiveRoleName = mapping?.RoleName;
            }

            var topMenusQuery = _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId == null);

            if (!string.IsNullOrWhiteSpace(module))
            {
                topMenusQuery = topMenusQuery.Where(m => m.MenuName != null && m.MenuName.ToLower() == module.ToLower());
            }

            var topMenus = await topMenusQuery.OrderBy(m => m.SerialNo).ToListAsync();
            var topMenuIds = topMenus.Select(m => m.Id).ToList();

            var subMenusQuery = _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId.HasValue && topMenuIds.Contains(m.ParentMenuId.Value));

            if (!string.IsNullOrWhiteSpace(area))
            {
                subMenusQuery = subMenusQuery.Where(m => m.ModuleName != null && m.ModuleName.ToLower() == area.ToLower());
            }

            var subMenus = await subMenusQuery.OrderBy(m => m.SerialNo).ToListAsync();

            var subMenuIds = subMenus.Select(s => s.Id).ToList();

            var grantedSubMenuIds = string.IsNullOrEmpty(effectiveRoleId)
                ? new HashSet<int>()
                : (await _context.RoleWiseMenuRights
                    .AsNoTracking()
                    .Where(r => r.RoleId == effectiveRoleId && subMenuIds.Contains(r.SubMenuId))
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
                        ModuleName = s.ModuleName,
                        IsGranted = grantedSubMenuIds.Contains(s.Id)
                    })
                    .ToList()
            }).ToList();

            return (effectiveRoleId, effectiveRoleName, groups);
        }

        // CHANGED — added `module` (required) and `area` (optional), and
        // `validSubMenus` is now scoped to that single top-level Module's
        // Id (plus Area, when supplied) instead of every Process/Reports
        // child everywhere. Without this scoping, saving one Module+Area
        // combination's checkboxes would treat every OTHER module's
        // existing grants for this role as "not requested" and delete
        // them — same bug class already fixed on the Dealer side.
        public async Task<(bool Success, string? Error)> UpdateLocationMenuAccessAsync(int locationId, string roleId, List<int> grantedSubMenuIds, string module, string? area, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return (false, "Please select a role.");

            if (string.IsNullOrWhiteSpace(module))
                return (false, "Please select a module.");

            var locExists = await _context.LocationMasters.AnyAsync(l => l.Id == locationId);
            if (!locExists) return (false, "Location not found.");

            var roleExists = await _context.AspNetRoles.AnyAsync(r => r.Id == roleId);
            if (!roleExists) return (false, "Selected role no longer exists.");

            var topMenu = await _context.MenuMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ParentMenuId == null && m.MenuName != null && m.MenuName.ToLower() == module.ToLower());

            if (topMenu == null)
                return (false, $"Module '{module}' not found.");

            var validSubMenusQuery = _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId == topMenu.Id);

            if (!string.IsNullOrWhiteSpace(area))
            {
                validSubMenusQuery = validSubMenusQuery.Where(m => m.ModuleName != null && m.ModuleName.ToLower() == area.ToLower());
            }

            var validSubMenus = await validSubMenusQuery.ToDictionaryAsync(m => m.Id, m => m.ParentMenuId!.Value);

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
    }
}