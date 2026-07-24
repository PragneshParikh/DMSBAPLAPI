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
        Task<bool> DeleteMapping(int id);
        Task<IdentityResult> UpdateMapping(int id, string name, string category);
    }
}