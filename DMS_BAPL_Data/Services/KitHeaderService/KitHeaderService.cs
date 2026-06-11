using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.KitHeaderRepo;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.KitHeaderService
{
    public partial class KitHeaderService : IKitHeaderService
    {
        private readonly IKitHeaderRepo _kitHeaderRepo;
        private readonly IExcelService _excelService;

        public KitHeaderService(IKitHeaderRepo kitHeaderRepo, IExcelService excelService)
        {
            _kitHeaderRepo = kitHeaderRepo;
            _excelService = excelService;
        }
        Task<PagedResponse<KitHeader>> IKitHeaderService.GetKitByPagedAsync(string? searchTerms, int pageIndex, int pageSize) => _kitHeaderRepo.GetKitByPagedAsync(searchTerms, pageIndex, pageSize);
        Task<int> IKitHeaderService.InsertKitHeader(KitHeader kitHeader) => _kitHeaderRepo.InsertKitHeader(kitHeader);
        Task<KitHeader?> IKitHeaderService.GetKitById(int id) => _kitHeaderRepo.GetKitById(id);
        Task<int> IKitHeaderService.UpdateKitHeader(KitHeader kitHeader) => _kitHeaderRepo.UpdateKitHeader(kitHeader);
        public async Task<byte[]> DownloadExcel()
        {
            try
            {
                var data = await _kitHeaderRepo.GetKitDetails();

                // Get all DTO properties for columns
                var properties = typeof(KitDetailExcelViewModel)
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
                    SheetName = "Kit",
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
