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
    }
}