using DMS_BAPL_Data.Repositories.ZoneMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ZoneMasterService
{
    public class ZoneMasterService : IZoneMasterService
    {
        private readonly IZoneMasterRepo _repo;

        public ZoneMasterService(IZoneMasterRepo repo)
            => _repo = repo;

        public Task<IEnumerable<ZoneViewModel>>
            GetAllZonesAsync()
                => _repo.GetAllZonesAsync();

        public Task<IEnumerable<ZoneDealerViewModel>>
            GetDealersByZoneAsync(string zone)
                => _repo.GetDealersByZoneAsync(zone);

        public Task<ZoneMasterViewModel>
            GetZoneByIdAsync(int id)
                => _repo.GetZoneByIdAsync(id);

        public Task<ZoneMasterViewModel>
            CreateZoneAsync(ZoneMasterViewModel model)
                => _repo.CreateZoneAsync(model);

        public Task<bool>
            UpdateZoneAsync(ZoneMasterViewModel model)
                => _repo.UpdateZoneAsync(model);

        public Task<bool>
            DeleteZoneAsync(int id)
                => _repo.DeleteZoneAsync(id);
    }
}