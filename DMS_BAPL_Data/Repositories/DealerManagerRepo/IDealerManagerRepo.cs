using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.DealerManagerRepo
{
    public enum DealerRoleAssignResult
    {
        Success,
        DealerNotFound,
        NoLinkedUser,
        RoleNotFound
    }

    public interface IDealerManagerRepo
    {
        Task<PagedResponse<DealerListViewModel>> GetAllAsync(DealerListFilterModel filter);
        Task<DealerListViewModel?> GetByIdAsync(int id);
        Task<bool> DealerCodeExistsAsync(string dealerCode, int excludeId);
        Task<bool> UpdateAsync(int id, DealerQuickUpdateViewModel model);
        Task<bool> DeactivateAsync(int id);
        Task<DealerRoleAssignResult> AssignRoleAsync(int dealerId, string roleId);

        Task<DealerMenuAccessResponseViewModel?> GetMenuAccessAsync(int dealerId, string? roleId);
        Task<(bool Success, string? Error)> UpdateMenuAccessAsync(int dealerId, string roleId, List<int> grantedSubMenuIds, string updatedBy);

        Task<List<DealerLocationViewModel>> GetLocationsAsync(int dealerId);
        Task<(bool Success, string? Error)> UpdateLocationsStatusAsync(int dealerId, List<int> locationIds, bool isActive, string updatedBy);
    }
}