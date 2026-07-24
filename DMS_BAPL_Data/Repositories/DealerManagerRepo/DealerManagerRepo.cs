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

            // Tracked (no AsNoTracking) — we're mutating this user's Roles
            // collection below, and SaveChangesAsync needs to see the change.
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

            // Single-role semantics, matching the popup's single Role field —
            // clears any existing role(s) on this user, then sets exactly the
            // one selected. This mutates the real AspNetUserRoles join table
            // via EF's skip-navigation tracking; no Identity manager needed.
            user.Roles.Clear();
            user.Roles.Add(role);

            await _context.SaveChangesAsync();
            return DealerRoleAssignResult.Success;
        }
    }
}