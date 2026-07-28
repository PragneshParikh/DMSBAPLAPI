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

            // Batch-load the linked login users for this page of dealers,
            // via the existing AspNetUser.DealerCode bridge — same one used
            // everywhere else in this app (storageService.getDealerCode()).
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

        public async Task<DealerMenuAccessResponseViewModel?> GetMenuAccessAsync(int dealerId, string? roleId)
        {
            var dealerExists = await _context.DealerMasters.AnyAsync(d => d.Id == dealerId);
            if (!dealerExists) return null;

            string? effectiveRoleId = roleId;
            string? effectiveRoleName = null;

            if (!string.IsNullOrWhiteSpace(effectiveRoleId))
            {
                // Explicit role picked from the modal's dropdown — look it up directly.
                var role = await _context.AspNetRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == effectiveRoleId);
                effectiveRoleName = role?.Name;
                if (role == null) effectiveRoleId = null; // bad/stale id — fall through as "no role selected"
            }
            else
            {
                // No role specified — default to whatever role is currently
                // assigned to this dealer's linked login, same as before.
                var (resolvedRoleId, resolvedRoleName) = await ResolveDealerRoleAsync(dealerId);
                effectiveRoleId = resolvedRoleId;
                effectiveRoleName = resolvedRoleName;
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

            return new DealerMenuAccessResponseViewModel
            {
                DealerId = dealerId,
                RoleId = effectiveRoleId,
                RoleName = effectiveRoleName,
                Groups = groups
            };
        }

        public async Task<(bool Success, string? Error)> UpdateMenuAccessAsync(int dealerId, string roleId, List<int> grantedSubMenuIds, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return (false, "Please select a role.");

            var dealerExists = await _context.DealerMasters.AnyAsync(d => d.Id == dealerId);
            if (!dealerExists)
                return (false, "Dealer not found.");

            var roleExists = await _context.AspNetRoles.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
                return (false, "Selected role no longer exists.");

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

            // Only ever touches locations that genuinely belong to THIS dealer —
            // a tampered payload can't flip another dealer's location status.
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