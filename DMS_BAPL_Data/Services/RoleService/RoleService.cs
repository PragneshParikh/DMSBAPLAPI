using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.RoleRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.RoleService
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepo _roleRepo;
        private readonly RoleManager<IdentityRole> _roleManager;   // ADD

        public RoleService(IRoleRepo roleRepo, RoleManager<IdentityRole> roleManager)  // ADD param
        {
            _roleRepo = roleRepo;
            _roleManager = roleManager;                            // ADD
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
        {
            return await _roleRepo.GetRoles();
        }

        public async Task<AspNetRole> GetRoleById(string roleId)
        {
            return await _roleRepo.GetRoleById(roleId);
        }

        public async Task<IdentityResult> CreateRoleWithCategory(RoleWithCategoryViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.Category))
                return IdentityResult.Failed(new IdentityError { Description = "Role name and category are required." });

            var name = vm.Name.Trim();
            var category = vm.Category.Trim();

            // create the role in AspNetRoles if it doesn't exist  (RoleManager, NOT repo)
            var role = await _roleManager.FindByNameAsync(name);
            if (role == null)
            {
                var create = await _roleManager.CreateAsync(new IdentityRole(name));
                if (!create.Succeeded) return create;
                role = await _roleManager.FindByNameAsync(name);
            }

            // map to category (skip if this role is already mapped to it)
            var existing = await _roleRepo.GetMappingsByCategory(category);
            if (!existing.Any(m => m.RoleId == role!.Id))
            {
                await _roleRepo.AddRoleCategoryMapping(new RoleCategoryMapping
                {
                    Category = category,
                    RoleId = role!.Id,
                    RoleName = role.Name!,
                    CreatedBy = "admin",
                    CreatedDate = DateTime.Now
                });
            }

            return IdentityResult.Success;
        }

        public async Task<List<RoleCategoryMapping>> GetRolesByCategory(string category)
        {
            return await _roleRepo.GetMappingsByCategory(category);
        }

        public async Task<List<RoleCategoryMapping>> GetAllMappings() => await _roleRepo.GetAllMappings();

        public async Task<bool> DeleteMapping(int id) => await _roleRepo.DeleteMapping(id);
    }
}