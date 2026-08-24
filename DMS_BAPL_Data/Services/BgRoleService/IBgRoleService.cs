using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Identity;
namespace DMS_BAPL_Data.Services.BgRoleService
{
    public interface IBgRoleService
    {
        Task<IEnumerable<AspNetRole>> GetRoles();
        Task<IdentityResult> CreateRoleWithCategory(BgRoleWithCategoryViewModel model, string createdBy);
        Task<List<BgRoleCategoryMapping>> GetRolesByCategory(string category);
        Task<List<BgRoleCategoryMapping>> GetAllMappings();
        Task<IdentityResult> UpdateMapping(int id, string name, string? category);
        Task<bool> DeleteMapping(int id);
  
        Task<LocationRoleDetailViewModel?> GetLocationDetailAsync(int locationId);
        Task<(bool Success, string? Error)> UpdateLocationDetailAsync(int locationId, UpdateLocationRoleDetailViewModel model, string updatedBy);

     
        Task<(string? RoleId, string? RoleName, List<DealerMenuAccessGroupViewModel> Groups)?> GetLocationMenuAccessAsync(int locationId, string? roleId, string? module, string? area);
        Task<(bool Success, string? Error)> UpdateLocationMenuAccessAsync(int locationId, string roleId, List<int> grantedSubMenuIds, string module, string? area, string updatedBy);
    }
}