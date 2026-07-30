using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.RoleRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.RoleService
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepo _roleRepo;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(IRoleRepo roleRepo, RoleManager<IdentityRole> roleManager)
        {
            _roleRepo = roleRepo;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
            => await _roleRepo.GetRoles();

        public async Task<IdentityResult> CreateRoleWithCategory(RoleWithCategoryViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Name))
            {
                var createResult = await _roleManager.CreateAsync(new IdentityRole(model.Name));
                if (!createResult.Succeeded)
                    return createResult;
            }
            var role = await _roleManager.FindByNameAsync(model.Name);
            await _roleRepo.AddRoleCategoryMapping(new RoleCategoryMapping
            {
                RoleId = role!.Id,
                RoleName = model.Name,
                Category = model.Category,
                CreatedDate = DateTime.Now
            });
            return IdentityResult.Success;
        }

        public async Task<List<RoleCategoryMapping>> GetAllMappings()
            => await _roleRepo.GetAllMappings();

        public async Task<bool> UpdateMapping(int id, string roleName, string? category)
            => await _roleRepo.UpdateMappingNameAndCategory(id, roleName, category);

        public async Task<bool> DeleteMapping(int id)
            => await _roleRepo.DeleteMapping(id);

        public async Task<RoleMenuAccessResponseViewModel?> GetMenuAccessAsync(string roleId)
            => await _roleRepo.GetMenuAccessAsync(roleId);

        public async Task<(bool Success, string? Error)> UpdateMenuAccessAsync(string roleId, List<int> grantedSubMenuIds, string updatedBy)
            => await _roleRepo.UpdateMenuAccessAsync(roleId, grantedSubMenuIds, updatedBy);

        public async Task<List<DealerMenuAccessGroupViewModel>> GetMenuTemplateAsync()
            => await _roleRepo.GetMenuTemplateAsync();

        public async Task<(string RoleId, string RoleName)?> ResolveOrCreateRoleForItemsAsync(string category, List<int> subMenuIds, string createdBy)
        {
            var requestedSet = (subMenuIds ?? new List<int>()).Distinct().ToHashSet();
            if (requestedSet.Count == 0) return null;

            var template = await _roleRepo.GetMenuTemplateAsync();
            var allValidItems = template.SelectMany(g => g.Items).ToList();
            var validIds = allValidItems.Select(i => i.SubMenuId).ToHashSet();
            requestedSet = requestedSet.Where(id => validIds.Contains(id)).ToHashSet();
            if (requestedSet.Count == 0) return null;

            var candidates = (await _roleRepo.GetAllMappings())
                .Where(m => string.Equals(m.Category, category, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(m.RoleId))
                .ToList();

            foreach (var candidate in candidates)
            {
                var access = await _roleRepo.GetMenuAccessAsync(candidate.RoleId!);
                if (access == null) continue;

                var grantedSet = access.Groups
                    .SelectMany(g => g.Items)
                    .Where(i => i.IsGranted)
                    .Select(i => i.SubMenuId)
                    .ToHashSet();

                if (grantedSet.SetEquals(requestedSet))
                    return (candidate.RoleId!, candidate.RoleName);
            }

            var name = string.Join(", ", allValidItems.Where(i => requestedSet.Contains(i.SubMenuId)).Select(i => i.MenuName));

            var createResult = await CreateRoleWithCategory(new RoleWithCategoryViewModel { Name = name, Category = category });
            if (!createResult.Succeeded) return null;

            var newMapping = (await _roleRepo.GetAllMappings())
                .Where(m => string.Equals(m.Category, category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(m => m.Id)
                .FirstOrDefault(m => string.Equals(m.RoleName, name, StringComparison.OrdinalIgnoreCase));

            if (newMapping?.RoleId == null) return null;

            var (success, _) = await _roleRepo.UpdateMenuAccessAsync(newMapping.RoleId, requestedSet.ToList(), createdBy);
            if (!success) return null;

            return (newMapping.RoleId, newMapping.RoleName);
        }
    }
}