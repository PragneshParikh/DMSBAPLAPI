using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.MenuRightsRepo
{
    public interface IMenuRightsRepo
    {
        Task<List<DealerDropdownViewModel>> GetDealersAsync();
        Task<(string? UserId, string? RoleId, string? RoleName)> GetDealerRoleAsync(string dealerCode);
        Task<List<MenuGroupViewModel>> GetMenuTreeWithRightsAsync(string? roleId);
        Task SaveMenuRightsAsync(string roleId, List<MenuRightItem> rights, string updatedBy);
    }
}