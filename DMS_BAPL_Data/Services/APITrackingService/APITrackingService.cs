using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.APITracking;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.APITrackingService
{
    public class APITrackingService : IAPITrackingService
    {
        private readonly IAPITrackingRepo _apiTrackingRepo;
        private readonly IExcelService _excelService;

        public APITrackingService(IAPITrackingRepo apiTrackingRepo, IExcelService excelService  )
        {
            _apiTrackingRepo = apiTrackingRepo;
            _excelService = excelService;
        }

        Task<List<Apitracking>> IAPITrackingService.GetAPITracking()
        {
            return _apiTrackingRepo.GetAPITracking();
        }

        Task<List<Apitracking>> IAPITrackingService.GetFilterRecords(DateTime fromDate, DateTime toDate, string endPoint, string searchCriteria, string status)
        {
            return _apiTrackingRepo.GetFilterRecords(fromDate, toDate, endPoint, searchCriteria, status);
        }


        public async Task<byte[]> DownloadAPIExcel()
        {
            try
            {
                var data = await _apiTrackingRepo.GetAPITracking();

                // Get all DTO properties for columns
                var properties = typeof(APITrackingViewModel)
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
                    SheetName = StringConstants.APIExcelSheetName,
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
