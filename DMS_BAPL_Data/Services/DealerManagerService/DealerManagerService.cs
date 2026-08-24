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
        public async Task<(bool Success, string? Error)> UnassignRoleAsync(int dealerId)
        {
            var result = await _repo.UnassignRoleAsync(dealerId);
            return result switch
            {
                DealerRoleAssignResult.Success => (true, null),
                DealerRoleAssignResult.DealerNotFound => (false, "Dealer not found."),
                DealerRoleAssignResult.NoLinkedUser => (false, "No login user account is linked to this dealer (matched by Dealer Code)."),
                DealerRoleAssignResult.RoleNotFound => (false, "Selected role no longer exists."),
                _ => (false, "Unable to remove role.")
            };
        }

        // CHANGED
        public async Task<List<string>> GetAvailableModulesAsync(string? area)
        {
            return await _repo.GetAvailableModulesAsync(area);
        }

        public async Task<List<string>> GetAvailableAreasAsync()
        {
            return await _repo.GetAvailableAreasAsync();
        }

        public async Task<DealerMenuAccessResponseViewModel?> GetMenuAccessAsync(int dealerId, string? roleId, string? module, string? area)
        {
            return await _repo.GetMenuAccessAsync(dealerId, roleId, module, area);
        }
        public async Task<(bool Success, string? Error)> UpdateMenuAccessAsync(int dealerId, string roleId, List<int> grantedSubMenuIds, string module, string? area, string updatedBy)
        {
            return await _repo.UpdateMenuAccessAsync(dealerId, roleId, grantedSubMenuIds ?? new List<int>(), module, area, updatedBy);
        }
        public async Task<List<DealerLocationViewModel>> GetLocationsAsync(int dealerId)
        {
            return await _repo.GetLocationsAsync(dealerId);
        }
        public async Task<(bool Success, string? Error)> UpdateLocationsStatusAsync(int dealerId, List<int> locationIds, bool isActive, string updatedBy)
        {
            if (locationIds == null || !locationIds.Any())
                return (false, "Select at least one location.");
            return await _repo.UpdateLocationsStatusAsync(dealerId, locationIds, isActive, updatedBy);
        }
    }
}