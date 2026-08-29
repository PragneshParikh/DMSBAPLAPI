using DMS_BAPL_Data.Repositories.MenuRightsRepo;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.MenuRightsService
{
    public class MenuRightsService : IMenuRightsService
    {
        private readonly IMenuRightsRepo _repo;

        public MenuRightsService(IMenuRightsRepo repo)
        {
            _repo = repo;
        }

        public Task<List<DealerDropdownViewModel>> GetDealers() => _repo.GetDealersAsync();

        public async Task<List<MenuGroupViewModel>> GetMenuRightsForDealer(string dealerCode)
        {
            var (_, roleId, _) = await _repo.GetDealerRoleAsync(dealerCode);
            return await _repo.GetMenuTreeWithRightsAsync(roleId);
        }

        public async Task<string?> SaveMenuRights(SaveMenuRightsRequest request, string updatedBy)
        {
            var (_, roleId, _) = await _repo.GetDealerRoleAsync(request.DealerCode);

            if (string.IsNullOrEmpty(roleId))
                return null;   // caller should treat null as "dealer has no role assigned"

            await _repo.SaveMenuRightsAsync(roleId, request.Rights, updatedBy);
            return roleId;
        }
    }
}