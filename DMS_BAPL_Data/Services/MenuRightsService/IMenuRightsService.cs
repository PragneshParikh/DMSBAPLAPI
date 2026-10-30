using DMS_BAPL_Data.Repositories.MenuRightsRepo;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.MenuRightsService
{
    public interface IMenuRightsService
    {
        Task<List<DealerDropdownViewModel>> GetDealers();
        Task<List<MenuGroupViewModel>> GetMenuRightsForDealer(string dealerCode);
        Task<string?> SaveMenuRights(SaveMenuRightsRequest request, string updatedBy);

        // Role-wise Menu Rights screen
        Task<List<MenuGroupViewModel>> GetMenuRightsForRole(string roleId);
        Task SaveRoleMenuRights(SaveRoleMenuRightsRequest request, string updatedBy);
    }
}