using DMS_BAPL_Utils.ViewModels;
using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Data.CustomModel;

namespace DMS_BAPL_Data.Repositories.DealerManagerRepo
{
    public class DealerManagerRepo : IDealerManagerRepo
    {
        private readonly BapldmsvadContext _context;

        public DealerManagerRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<DealerListViewModel>> GetAllAsync(DealerListFilterModel filter)
        {
            var query = _context.DealerMasters.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.Dealercode == filter.DealerCode);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(x =>
                    (x.Dealercode != null && x.Dealercode.Contains(s)) ||
                    (x.Compname != null && x.Compname.Contains(s)) ||
                    (x.Email != null && x.Email.Contains(s)));
            }

            var totalRecords = await query.CountAsync();

            var pagedDealers = await query
                .OrderByDescending(x => x.Id)
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dealerCodes = pagedDealers
                .Where(d => !string.IsNullOrWhiteSpace(d.Dealercode))
                .Select(d => d.Dealercode!)
                .Distinct()
                .ToList();

            var linkedUsers = dealerCodes.Count == 0
                ? new List<AspNetUser>()
                : await _context.AspNetUsers
                    .AsNoTracking()
                    .Include(u => u.Roles)
                    .Where(u => u.DealerCode != null && dealerCodes.Contains(u.DealerCode))
                    .ToListAsync();

            var userLookup = linkedUsers
                .GroupBy(u => u.DealerCode!)
                .ToDictionary(g => g.Key, g => g.OrderBy(u => u.Id).First());

            var data = pagedDealers.Select(d =>
            {
                AspNetUser? user = d.Dealercode != null && userLookup.TryGetValue(d.Dealercode, out var u) ? u : null;
                var role = user?.Roles.FirstOrDefault();

                return new DealerListViewModel
                {
                    Id = d.Id,
                    Dealercode = d.Dealercode,
                    Compname = d.Compname,
                    Email = d.Email,
                    CreatedDate = d.CreatedDate,
                    IsActive = d.IsActive,
                    LinkedUserId = user?.Id,
                    LinkedUserName = user?.UserName,
                    RoleId = role?.Id,
                    RoleName = role?.Name
                };
            }).ToList();

            return new PagedResponse<DealerListViewModel>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        public async Task<DealerListViewModel?> GetByIdAsync(int id)
        {
            var dealer = await _context.DealerMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (dealer == null) return null;

            AspNetUser? user = null;
            if (!string.IsNullOrWhiteSpace(dealer.Dealercode))
            {
                user = await _context.AspNetUsers
                    .AsNoTracking()
                    .Include(u => u.Roles)
                    .Where(u => u.DealerCode == dealer.Dealercode)
                    .OrderBy(u => u.Id)
                    .FirstOrDefaultAsync();
            }

            var role = user?.Roles.FirstOrDefault();

            return new DealerListViewModel
            {
                Id = dealer.Id,
                Dealercode = dealer.Dealercode,
                Compname = dealer.Compname,
                Email = dealer.Email,
                CreatedDate = dealer.CreatedDate,
                IsActive = dealer.IsActive,
                LinkedUserId = user?.Id,
                LinkedUserName = user?.UserName,
                RoleId = role?.Id,
                RoleName = role?.Name
            };
        }

        public async Task<bool> DealerCodeExistsAsync(string dealerCode, int excludeId)
        {
            return await _context.DealerMasters
                .AnyAsync(x => x.Dealercode == dealerCode && x.Id != excludeId);
        }

        public async Task<bool> UpdateAsync(int id, DealerQuickUpdateViewModel model)
        {
            var entity = await _context.DealerMasters.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            entity.Dealercode = model.Dealercode;
            entity.Compname = model.Compname;
            entity.Email = model.Email;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var entity = await _context.DealerMasters.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DealerRoleAssignResult> AssignRoleAsync(int dealerId, string roleId)
        {
            var dealer = await _context.DealerMasters.FirstOrDefaultAsync(d => d.Id == dealerId);
            if (dealer == null || string.IsNullOrWhiteSpace(dealer.Dealercode))
                return DealerRoleAssignResult.DealerNotFound;
            var user = await _context.AspNetUsers
                .Include(u => u.Roles)
                .Where(u => u.DealerCode == dealer.Dealercode)
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();

            if (user == null)
                return DealerRoleAssignResult.NoLinkedUser;

            var role = await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                return DealerRoleAssignResult.RoleNotFound;
            user.Roles.Clear();
            user.Roles.Add(role);

            await _context.SaveChangesAsync();
            return DealerRoleAssignResult.Success;
        }

        public async Task<DealerRoleAssignResult> UnassignRoleAsync(int dealerId)
        {
            var dealer = await _context.DealerMasters.FirstOrDefaultAsync(d => d.Id == dealerId);
            if (dealer == null || string.IsNullOrWhiteSpace(dealer.Dealercode))
                return DealerRoleAssignResult.DealerNotFound;

            var user = await _context.AspNetUsers
                .Include(u => u.Roles)
                .Where(u => u.DealerCode == dealer.Dealercode)
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();

            if (user == null)
                return DealerRoleAssignResult.NoLinkedUser;

            user.Roles.Clear();
            await _context.SaveChangesAsync();
            return DealerRoleAssignResult.Success;
        }

        private async Task<(string? RoleId, string? RoleName)> ResolveDealerRoleAsync(int dealerId)
        {
            var dealer = await _context.DealerMasters.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dealerId);
            if (dealer == null || string.IsNullOrWhiteSpace(dealer.Dealercode))
                return (null, null);

            var user = await _context.AspNetUsers
                .AsNoTracking()
                .Include(u => u.Roles)
                .Where(u => u.DealerCode == dealer.Dealercode)
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();

            var role = user?.Roles.FirstOrDefault();
            return (role?.Id, role?.Name);
        }

        // CHANGED — now accepts an optional `area`. Final hierarchy is
        // Role -> Area -> Module, so once an Area (ShowRoom/WorkShop/
        // Account) is chosen, the Module dropdown must only offer modules
        // that actually contain at least one form tagged with that Area —
        // "Area-wise Module". Passing null/empty preserves the original
        // "every top-level module" behavior for any other caller.
        public async Task<List<string>> GetAvailableModulesAsync(string? area)
        {
            var topMenus = await _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId == null && m.MenuName != null)
                .OrderBy(m => m.SerialNo)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(area))
            {
                return topMenus.Select(m => m.MenuName!).Distinct().ToList();
            }

            var topMenuIds = topMenus.Select(m => m.Id).ToList();

            var moduleIdsWithArea = await _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId.HasValue
                         && topMenuIds.Contains(m.ParentMenuId.Value)
                         && m.ModuleName != null
                         && m.ModuleName.ToLower() == area.ToLower())
                .Select(m => m.ParentMenuId!.Value)
                .Distinct()
                .ToListAsync();

            return topMenus
                .Where(m => moduleIdsWithArea.Contains(m.Id))
                .Select(m => m.MenuName!)
                .ToList();
        }

        public async Task<List<string>> GetAvailableAreasAsync()
        {
            return await _context.MenuMasters
                .AsNoTracking()
                .Where(m => m.ParentMenuId != null && m.ModuleName != null && m.ModuleName != "")
                .Select(m => m.ModuleName!)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        public async Task<DealerMenuAccessResponseViewModel?> GetMenuAccessAsync(int dealerId, string? roleId, string? module, string? area)
        {
            var dealerExists = await _context.DealerMasters.AnyAsync(d => d.Id == dealerId);
            if (!dealerExists) return null;

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
                var (resolvedRoleId, resolvedRoleName) = await ResolveDealerRoleAsync(dealerId);
                effectiveRoleId = resolvedRoleId;
                effectiveRoleName = resolvedRoleName;
            }

            var groups = await BuildMenuAccessGroupsAsync(effectiveRoleId, module, area);

            return new DealerMenuAccessResponseViewModel
            {
                DealerId = dealerId,
                RoleId = effectiveRoleId,
                RoleName = effectiveRoleName,
                Groups = groups
            };
        }

        // NEW — used by DealerManagerController.GetMyAccess for a Location
        // Login session. That call has a roleId straight from the JWT's
        // LocationRoleId claim and no dealerId at all, so it can't go through
        // GetMenuAccessAsync above: that method's dealerId-exists guard
        // (`_context.DealerMasters.AnyAsync(d => d.Id == dealerId)`) would
        // never pass for a placeholder id like 0, making this resolve to
        // null every time regardless of what the location was actually
        // granted. This skips that guard entirely — a location's role
        // doesn't need a dealerId to be valid.
        public async Task<DealerMenuAccessResponseViewModel?> GetMenuAccessByRoleIdAsync(string roleId, string? module, string? area)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return null;

            var role = await _context.AspNetRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                return null;

            var groups = await BuildMenuAccessGroupsAsync(roleId, module, area);

            return new DealerMenuAccessResponseViewModel
            {
                DealerId = 0,
                RoleId = roleId,
                RoleName = role.Name,
                Groups = groups
            };
        }

        // NEW — the actual Module/Area/RoleWiseMenuRights tree-building logic,
        // extracted out of GetMenuAccessAsync so GetMenuAccessByRoleIdAsync can
        // reuse it verbatim instead of duplicating it (and risking the two
        // drifting apart later).
        private async Task<List<DealerMenuAccessGroupViewModel>> BuildMenuAccessGroupsAsync(string? effectiveRoleId, string? module, string? area)
        {
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
                        ModuleName = s.ModuleName,
                        IsGranted = grantedSubMenuIds.Contains(s.Id)
                    })
                    .ToList()
            }).ToList();
        }

        public async Task<(bool Success, string? Error)> UpdateMenuAccessAsync(int dealerId, string roleId, List<int> grantedSubMenuIds, string module, string? area, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return (false, "Please select a role.");

            if (string.IsNullOrWhiteSpace(module))
                return (false, "Please select a module.");

            var dealerExists = await _context.DealerMasters.AnyAsync(d => d.Id == dealerId);
            if (!dealerExists)
                return (false, "Dealer not found.");

            var roleExists = await _context.AspNetRoles.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
                return (false, "Selected role no longer exists.");

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

        public async Task<List<DealerLocationViewModel>> GetLocationsAsync(int dealerId)
        {
            var dealer = await _context.DealerMasters.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dealerId);
            if (dealer == null || string.IsNullOrWhiteSpace(dealer.Dealercode))
                return new List<DealerLocationViewModel>();

            return await _context.LocationMasters
                .AsNoTracking()
                .Where(l => l.Dealercode == dealer.Dealercode)
                .Select(l => new DealerLocationViewModel
                {
                    Id = l.Id,
                    LocCode = l.Loccode,
                    LocName = l.Locname,
                    IsActive = l.Active == "Y"
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Error)> UpdateLocationsStatusAsync(int dealerId, List<int> locationIds, bool isActive, string updatedBy)
        {
            var dealer = await _context.DealerMasters.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dealerId);
            if (dealer == null || string.IsNullOrWhiteSpace(dealer.Dealercode))
                return (false, "Dealer not found.");

            var locations = await _context.LocationMasters
                .Where(l => locationIds.Contains(l.Id) && l.Dealercode == dealer.Dealercode)
                .ToListAsync();

            if (!locations.Any())
                return (false, "No matching locations found for this dealer.");

            foreach (var loc in locations)
            {
                loc.Active = isActive ? "Y" : "N";
                loc.UpdatedBy = updatedBy;
                loc.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}