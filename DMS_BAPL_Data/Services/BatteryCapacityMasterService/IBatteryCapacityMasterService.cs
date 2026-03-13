using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.BatteryCapacityMasterService
{
    public interface IBatteryCapacityMasterService
    {
        Task<BatteryCapacityMaster> AddBatteryCapacityMasterAsync(BatteryCapacityMasterViewModel batteryCapacityMaster);
        Task<List<BatteryCapacityMaster>> GetBatteryCapacityMastersAsync();
        Task<BatteryCapacityMaster?> UpdateBatteryCapacityMasterAsync(int id, BatteryCapacityMasterViewModel batteryCapacityMasterViewModel);
        Task<PagedResponseBattery<BatteryCapacityMaster>> GetPaginatedBatteryCapacityMastersAsync(string? batteryCapacity, int? page, int? pageSize);
        Task<byte[]> DownloadDealerExcel();
    }
}
