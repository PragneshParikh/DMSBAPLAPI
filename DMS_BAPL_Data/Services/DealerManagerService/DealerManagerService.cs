using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.DealerManagerRepo;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Services.DealerManagerService
{
    public class DealerManagerService : IDealerManagerService
    {
        private readonly IDealerManagerRepo _repo;

        public DealerManagerService(IDealerManagerRepo repo)
        {
            _repo = repo;
        }

        public async Task<PagedResponse<DealerListViewModel>> GetAllAsync(DealerListFilterModel filter)
        {
            return await _repo.GetAllAsync(filter);
        }

        public async Task<DealerListViewModel?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, DealerQuickUpdateViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Dealercode))
                return (false, "Dealer code is required.");

            if (string.IsNullOrWhiteSpace(model.Compname))
                return (false, "Company name is required.");

            if (await _repo.DealerCodeExistsAsync(model.Dealercode.Trim(), id))
                return (false, $"Dealer code '{model.Dealercode}' is already used by another dealer.");

            var ok = await _repo.UpdateAsync(id, model);
            return ok ? (true, null) : (false, "Dealer not found.");
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            return await _repo.DeactivateAsync(id);
        }

        public async Task<(bool Success, string? Error)> AssignRoleAsync(int dealerId, string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return (false, "Please select a role.");

            var result = await _repo.AssignRoleAsync(dealerId, roleId);

            return result switch
            {
                DealerRoleAssignResult.Success => (true, null),
                DealerRoleAssignResult.DealerNotFound => (false, "Dealer not found."),
                DealerRoleAssignResult.NoLinkedUser => (false, "No login user account is linked to this dealer (matched by Dealer Code) — a role can only be assigned once a user account exists for it."),
                DealerRoleAssignResult.RoleNotFound => (false, "Selected role no longer exists."),
                _ => (false, "Unable to assign role.")
            };
        }
    }
}