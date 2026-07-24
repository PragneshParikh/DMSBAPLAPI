using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.BgRoleRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DMS_BAPL_Data.Services.BgRoleService
{
    public class BgRoleService : IBgRoleService
    {
        private readonly IBgRoleRepo _bgRoleRepo;
        private readonly RoleManager<IdentityRole> _roleManager;

        public BgRoleService(IBgRoleRepo bgRoleRepo, RoleManager<IdentityRole> roleManager)
        {
            _bgRoleRepo = bgRoleRepo;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
        {
            return await _bgRoleRepo.GetRoles();
        }

        public async Task<IdentityResult> CreateRoleWithCategory(BgRoleWithCategoryViewModel model, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Category))
                return IdentityResult.Failed(new IdentityError { Description = "Role name and category are required." });

            var name = model.Name.Trim();
            var category = model.Category.Trim();

            var role = await _roleManager.FindByNameAsync(name);
            if (role == null)
            {
                var createResult = await _roleManager.CreateAsync(new IdentityRole(name));
                if (!createResult.Succeeded)
                    return createResult;

                role = await _roleManager.FindByNameAsync(name);
            }

          
            var existing = await _bgRoleRepo.GetMappingsByCategory(category);
            if (!existing.Any(m => m.RoleId == role!.Id))
            {
                await _bgRoleRepo.AddRoleCategoryMapping(new BgRoleCategoryMapping
                {
                    RoleId = role!.Id,
                    RoleName = role.Name!,
                    Category = category,
                    CreatedBy = createdBy,   // real logged-in user, not hardcoded
                    CreatedDate = DateTime.Now
                });
            }

            return IdentityResult.Success;
        }

        public async Task<List<BgRoleCategoryMapping>> GetRolesByCategory(string category)
        {
            return await _bgRoleRepo.GetMappingsByCategory(category);
        }

        public async Task<List<BgRoleCategoryMapping>> GetAllMappings()
        {
            return await _bgRoleRepo.GetAllMappings();
        }

        public async Task<IdentityResult> UpdateMapping(int id, string name, string category)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
                return IdentityResult.Failed(new IdentityError { Description = "Role name and category are required." });

            var trimmedName = name.Trim();
            var trimmedCategory = category.Trim();

            var mapping = await _bgRoleRepo.GetMappingById(id);
            if (mapping == null)
                return IdentityResult.Failed(new IdentityError { Description = "Mapping not found." });
            if (!string.IsNullOrEmpty(mapping.RoleId) &&
                !string.Equals(mapping.RoleName, trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                var role = await _roleManager.FindByIdAsync(mapping.RoleId);
                if (role == null)
                    return IdentityResult.Failed(new IdentityError { Description = "Underlying Identity role not found." });

                // Block renaming into a name already owned by a DIFFERENT role.
                var clash = await _roleManager.FindByNameAsync(trimmedName);
                if (clash != null && clash.Id != role.Id)
                    return IdentityResult.Failed(new IdentityError { Description = $"A role named '{trimmedName}' already exists." });
                var renameResult = await _roleManager.SetRoleNameAsync(role, trimmedName);
                if (!renameResult.Succeeded)
                    return renameResult;
            }

            await _bgRoleRepo.UpdateMappingNameAndCategory(id, trimmedName, trimmedCategory);
            return IdentityResult.Success;
        }
        public async Task<bool> DeleteMapping(int id)
        {
            return await _bgRoleRepo.DeleteMapping(id);
        }
    }
}