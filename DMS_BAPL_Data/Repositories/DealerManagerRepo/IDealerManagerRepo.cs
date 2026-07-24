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

        // NEW — no new table; reassigns the role of the AspNetUser linked
        // to this dealer via DealerCode.
        Task<DealerRoleAssignResult> AssignRoleAsync(int dealerId, string roleId);
    }
}