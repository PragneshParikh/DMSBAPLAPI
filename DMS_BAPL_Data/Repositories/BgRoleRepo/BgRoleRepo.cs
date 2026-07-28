using DMS_BAPL_Data.DBModels;
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
            // CHANGED — excludes location-specific assignment rows (LocationId
            // populated), so a location's own role assignment never shows up
            // as a phantom extra entry in the general registered-role list.
            return await _context.BgRoleCategoryMappings
                .Where(m => m.Category == category && m.LocationId == null)
                .ToListAsync();
        }

        public async Task<List<BgRoleCategoryMapping>> GetAllMappings()
        {
            // CHANGED — same LocationId == null filter. This backs BG Role
            // Master's own list screen AND every role-search dropdown
            // (Dealer Edit, Location Edit) — all of them must only ever see
            // the general registered roles, never per-location assignments.
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
                // The chosen role must already exist as a registered BG Role
                // Master entry (a row with this RoleId and LocationId == null) —
                // same validation boundary the Dealer feature already enforces.
                var registered = await _context.BgRoleCategoryMappings
                    .FirstOrDefaultAsync(m => m.RoleId == model.RoleId && m.LocationId == null);

                if (registered == null)
                    return (false, "Selected role is not registered in BG Role Master.");

                // Upsert this location's own assignment row.
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

        public async Task<(string? RoleId, string? RoleName, List<DealerMenuAccessGroupViewModel> Groups)?> GetLocationMenuAccessAsync(int locationId, string? roleId)
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
                        IsGranted = grantedSubMenuIds.Contains(s.Id)
                    })
                    .ToList()
            }).ToList();

            return (effectiveRoleId, effectiveRoleName, groups);
        }

        public async Task<(bool Success, string? Error)> UpdateLocationMenuAccessAsync(int locationId, string roleId, List<int> grantedSubMenuIds, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return (false, "Please select a role.");

            var locExists = await _context.LocationMasters.AnyAsync(l => l.Id == locationId);
            if (!locExists) return (false, "Location not found.");

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
    }
}