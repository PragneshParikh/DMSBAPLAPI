using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ZoneMasterRepo
{
    public interface IZoneMasterRepo
    {
        Task<IEnumerable<ZoneViewModel>> GetAllZonesAsync();
        Task<IEnumerable<ZoneDealerViewModel>> GetDealersByZoneAsync(string zone);
        Task<ZoneMasterViewModel> GetZoneByIdAsync(int id);
        Task<ZoneMasterViewModel> CreateZoneAsync(ZoneMasterViewModel model);
        Task<bool> UpdateZoneAsync(ZoneMasterViewModel model);
        Task<bool> DeleteZoneAsync(int id);
    }
}