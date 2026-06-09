using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ExtendedBatteryWarrantyRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ExtendedBatteryWarrantyService
{
    public partial class ExtendedBatteryWarrantyService : IExtendedBatteryWarrantyService
    {
        private readonly IExtendedBatteryWarrantyRepo _extendedBatteryWarrantyRepo;
        private readonly IExcelService _excelService;

        public ExtendedBatteryWarrantyService(IExtendedBatteryWarrantyRepo extendedBatteryWarrantyRepo, IExcelService excelService)
        {
            _extendedBatteryWarrantyRepo = extendedBatteryWarrantyRepo;
            _excelService = excelService;
        }

        Task<IEnumerable<ExtendedBatteryWarranty>> IExtendedBatteryWarrantyService.Get() => _extendedBatteryWarrantyRepo.Get();
        Task<PagedResponse<object>> IExtendedBatteryWarrantyService.GetExtendedBatteryWarrantyByPaged(string? searchTerm, int pageIndex, int pageSize) => _extendedBatteryWarrantyRepo.GetExtendedBatteryWarrantyByPaged(searchTerm, pageIndex, pageSize);
        Task<ExtendedBatteryWarranty?> IExtendedBatteryWarrantyService.GetSchemeDetailById(int id) => _extendedBatteryWarrantyRepo.GetSchemeDetailById(id);
        int IExtendedBatteryWarrantyService.Insert(ExtendedBatteryWarrantyViewModel extendedBatteryWarrantyViewModel) => _extendedBatteryWarrantyRepo.Insert(extendedBatteryWarrantyViewModel);
        Task<int> IExtendedBatteryWarrantyService.Update(ExtendedBatteryWarrantyViewModel extendedBatteryWarrntyViewModel) => _extendedBatteryWarrantyRepo.Update(extendedBatteryWarrntyViewModel);
        public async Task<byte[]> DownloadExcel()
        {
            try
            {
                var data = await _extendedBatteryWarrantyRepo.Get();

                // Get all DTO properties for columns
                var properties = typeof(ExtendedBatteryWarrantyExcelViewModel)
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
                    SheetName = "ExtendedBatteryWarranty",
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
