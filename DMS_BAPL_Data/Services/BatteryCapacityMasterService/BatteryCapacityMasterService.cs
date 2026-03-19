using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.BatteryCapacityMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.BatteryCapacityMasterService
{
    public class BatteryCapacityMasterService : IBatteryCapacityMasterService
    {
        private readonly IBatteryCapacityMasterRepo _batteryCapacityMasterRepo;
        private readonly IExcelService _excelService;
        public BatteryCapacityMasterService(IBatteryCapacityMasterRepo batteryCapacityMasterRepo, IExcelService excelService    )
        {
            _batteryCapacityMasterRepo = batteryCapacityMasterRepo;
            _excelService = excelService;
        }

        public async Task<BatteryCapacityMaster> AddBatteryCapacityMasterAsync(BatteryCapacityMasterViewModel batteryCapacityMaster, string userId)
        {
            return await _batteryCapacityMasterRepo.AddBatteryCapacityMasterAsync(batteryCapacityMaster,userId);

        }

        public async Task<List<BatteryCapacityMaster>> GetBatteryCapacityMastersAsync()
        {
            return await _batteryCapacityMasterRepo.GetBatteryCapacityMastersAsync();
        }
        public async Task<BatteryCapacityMaster?> UpdateBatteryCapacityMasterAsync(int id, BatteryCapacityMasterViewModel batteryCapacityMasterViewModel,string userId)
        {
            return await _batteryCapacityMasterRepo.UpdateBatteryCapacityMasterAsync(id, batteryCapacityMasterViewModel,userId);
        }
        public async Task<PagedResponseBattery<BatteryCapacityMaster>> GetPaginatedBatteryCapacityMastersAsync(string? batteryCapacity, int? page, int? pageSize)
        {
            return await _batteryCapacityMasterRepo.GetPaginatedBatteryCapacityMastersAsync(batteryCapacity, page, pageSize);
        }

        public async Task<byte[]> DownloadDealerExcel()
        {
            try
            {
                var data = await _batteryCapacityMasterRepo.GetBatteryCapacityMastersAsync();

                // Get all DTO properties for columns
                var properties = typeof(BatteryCapacityMasterViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);

                        if (entityProp != null)
                            dict[prop.Name] = entityProp.GetValue(d);
                        else
                            dict[prop.Name] = null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.BatteryCapacityMasterExcelSheetName,
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
