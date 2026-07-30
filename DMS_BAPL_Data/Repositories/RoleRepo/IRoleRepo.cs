using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.RoleRepo
{
    public interface IRoleRepo
    {
        Task<IEnumerable<AspNetRole>> GetRoles();
        Task AddRoleCategoryMapping(RoleCategoryMapping mapping);
        Task<List<RoleCategoryMapping>> GetAllMappings();
        Task<bool> UpdateMappingNameAndCategory(int id, string roleName, string? category);
        Task<bool> DeleteMapping(int id);

        // Menu access — Sale/Service roles only
        Task<RoleMenuAccessResponseViewModel?> GetMenuAccessAsync(string roleId);
        Task<(bool Success, string? Error)> UpdateMenuAccessAsync(string roleId, List<int> grantedSubMenuIds, string updatedBy);

        // Menu template — full unchecked Process/Reports list, no role required yet
        Task<List<DealerMenuAccessGroupViewModel>> GetMenuTemplateAsync();
    }
}