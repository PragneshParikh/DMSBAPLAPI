using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.BgRoleRepo
{
    public interface IBgRoleRepo
    {
        Task<IEnumerable<AspNetRole>> GetRoles();
        Task<AspNetRole?> GetRoleById(string id);
        Task AddRoleCategoryMapping(BgRoleCategoryMapping mapping);
        Task<List<BgRoleCategoryMapping>> GetMappingsByCategory(string category);
        Task<List<BgRoleCategoryMapping>> GetAllMappings();
        Task<BgRoleCategoryMapping?> GetMappingById(int id);
        Task<bool> UpdateMappingNameAndCategory(int id, string roleName, string? category);
        Task<bool> DeleteMapping(int id);
        Task<LocationRoleDetailViewModel?> GetLocationDetailAsync(int locationId);
        Task<(bool Success, string? Error)> UpdateLocationDetailAsync(int locationId, UpdateLocationRoleDetailViewModel model, string updatedBy);
        Task<(string? RoleId, string? RoleName, List<DealerMenuAccessGroupViewModel> Groups)?> GetLocationMenuAccessAsync(int locationId, string? roleId);
        Task<(bool Success, string? Error)> UpdateLocationMenuAccessAsync(int locationId, string roleId, List<int> grantedSubMenuIds, string updatedBy);
    }
}