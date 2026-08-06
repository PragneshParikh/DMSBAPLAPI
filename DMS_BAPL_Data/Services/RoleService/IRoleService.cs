using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.RoleService
{
    public interface IRoleService
    {
        Task<IEnumerable<AspNetRole>> GetRoles();
        Task<IdentityResult> CreateRoleWithCategory(RoleWithCategoryViewModel model);
        Task<List<RoleCategoryMapping>> GetAllMappings();
        Task<bool> UpdateMapping(int id, string roleName, string? category);
        Task<bool> DeleteMapping(int id);
        Task<List<RoleCategoryMapping>> GetMappingsByCategory(string category);
        Task<RoleMenuAccessResponseViewModel?> GetMenuAccessAsync(string roleId);
        Task<(bool Success, string? Error)> UpdateMenuAccessAsync(string roleId, List<int> grantedSubMenuIds, string updatedBy);
        Task<List<DealerMenuAccessGroupViewModel>> GetMenuTemplateAsync();
        Task<(string RoleId, string RoleName)?> ResolveOrCreateRoleForItemsAsync(string category, List<int> subMenuIds, string createdBy);
    }
}