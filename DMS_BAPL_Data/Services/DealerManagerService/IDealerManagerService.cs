using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Utils.ViewModels;
namespace DMS_BAPL_Data.Services.DealerManagerService
{
    public interface IDealerManagerService
    {
        Task<PagedResponse<DealerListViewModel>> GetAllAsync(DealerListFilterModel filter);
        Task<DealerListViewModel?> GetByIdAsync(int id);
        Task<(bool Success, string? Error)> UpdateAsync(int id, DealerQuickUpdateViewModel model);
        Task<bool> DeactivateAsync(int id);
        Task<(bool Success, string? Error)> AssignRoleAsync(int dealerId, string roleId);
        Task<(bool Success, string? Error)> UnassignRoleAsync(int dealerId);
        Task<List<string>> GetAvailableModulesAsync(string? area);
        Task<List<string>> GetAvailableAreasAsync();
        Task<DealerMenuAccessResponseViewModel?> GetMenuAccessAsync(int dealerId, string? roleId, string? module, string? area);
        Task<(bool Success, string? Error)> UpdateMenuAccessAsync(int dealerId, string roleId, List<int> grantedSubMenuIds, string module, string? area, string updatedBy);
        Task<List<DealerLocationViewModel>> GetLocationsAsync(int dealerId);
        Task<(bool Success, string? Error)> UpdateLocationsStatusAsync(int dealerId, List<int> locationIds, bool isActive, string updatedBy);
    }
}