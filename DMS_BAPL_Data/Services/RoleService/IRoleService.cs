using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.RoleService
{
    public interface IRoleService
    {
        Task<IEnumerable<AspNetRole>> GetRoles();
        Task<AspNetRole> GetRoleById(string id);

        Task<IdentityResult> CreateRoleWithCategory(RoleWithCategoryViewModel vm);
        Task<List<RoleCategoryMapping>> GetRolesByCategory(string category);

        Task<List<RoleCategoryMapping>> GetAllMappings();
        Task<bool> DeleteMapping(int id);
    }
}
