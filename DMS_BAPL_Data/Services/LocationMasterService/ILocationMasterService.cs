using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LocationMasterService
{
    public interface ILocationMasterService
    {
        Task<List<LocationMasterViewModel>> GetAllLocationMaster();
        Task<LocationMasterViewModel> GetLocationMasterById(int id);
        Task<bool> AddLocationMaster(LocationMasterViewModel model);
        Task<bool> UpdateLocationMaster(LocationMasterViewModel model);
        Task<byte[]> DownloadLocationMasterExcel();
        Task<List<LocationNameViewModel>> GetLocationByDealerCode(string dealerCode);
        Task<List<LocationTypewiseNameViewModel>> GetLocationNameTypewiseListAsync(string dealerCode);
        Task<object> UpdateByLocationCode(string userId, LocationMasterViewModel locationMasterViewModel);
        Task<IEnumerable<LocationNameViewModel>> GetLocationByDealerByAreaId(string? dealerCode, int areaId);
        Task<IEnumerable<object>> GetDealerPrimaryLocationByAreaId(int areaId, string locCode, string? dealerCode);
        Task<List<LocationNameViewModel>> GetAllLocationByDealerCode(string dealerCode);
        Task<IEnumerable<LocationMasterViewModel>> GetLocationDropdownByDealerCode(string? dealerCode);
    }
}
