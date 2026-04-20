using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.BatteryCapacityMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.BatteryCapacityMasterService
{
    public class BatteryCapacityMasterService : IBatteryCapacityMasterService
    {
        private readonly IBatteryCapacityMasterRepo _batteryCapacityMasterRepo;
        private readonly IExcelService _excelService;

        public BatteryCapacityMasterService(
            IBatteryCapacityMasterRepo batteryCapacityMasterRepo,
            IExcelService excelService)
        {
            _batteryCapacityMasterRepo = batteryCapacityMasterRepo;
            _excelService = excelService;
        }

        // Add new battery capacity record
        public async Task<BatteryCapacityMaster> AddBatteryCapacityMasterAsync(
            BatteryCapacityMasterViewModel batteryCapacityMaster,
            string userId)
        {
            try
            {
                return await _batteryCapacityMasterRepo
                    .AddBatteryCapacityMasterAsync(batteryCapacityMaster, userId);
            }
            catch
            {
                throw;
            }
        }

        // Get all battery capacity records
        public async Task<List<BatteryCapacityMaster>> GetBatteryCapacityMastersAsync()
        {
            try
            {
                return await _batteryCapacityMasterRepo
                    .GetBatteryCapacityMastersAsync();
            }
            catch
            {
                throw;
            }
        }

        // Update battery capacity record
        public async Task<BatteryCapacityMaster?> UpdateBatteryCapacityMasterAsync(
            int id,
            BatteryCapacityMasterViewModel batteryCapacityMasterViewModel,
            string userId)
        {
            try
            {
                return await _batteryCapacityMasterRepo
                    .UpdateBatteryCapacityMasterAsync(id, batteryCapacityMasterViewModel, userId);
            }
            catch
            {
                throw;
            }
        }

        // Get paginated battery capacity data
        public async Task<PagedResponseBattery<BatteryCapacityMaster>> GetPaginatedBatteryCapacityMastersAsync(
            string? batteryCapacity,
            int? page,
            int? pageSize)
        {
            try
            {
                return await _batteryCapacityMasterRepo
                    .GetPaginatedBatteryCapacityMastersAsync(batteryCapacity, page, pageSize);
            }
            catch
            {
                throw;
            }
        }

        // Export battery capacity data to Excel
        public async Task<byte[]> DownloadDealerExcel()
        {
            try
            {
                var data = await _batteryCapacityMasterRepo
                    .GetBatteryCapacityMastersAsync();

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

                        dict[prop.Name] = entityProp != null
                            ? entityProp.GetValue(d)
                            : null;
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