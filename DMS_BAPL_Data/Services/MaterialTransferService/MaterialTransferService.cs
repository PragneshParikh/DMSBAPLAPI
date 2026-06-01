using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.MaterialTransferRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.MaterialTransferService
{
    public partial class MaterialTransferService : IMaterialTransferService
    {
        private readonly IMaterialTransferRepo _materialTransferRepo;
        private readonly IExcelService _excelService;

        public MaterialTransferService(IMaterialTransferRepo materialTransferRepo, IExcelService excelService)
        {
            _materialTransferRepo = materialTransferRepo;
            _excelService = excelService;
        }

        Task<object> IMaterialTransferService.Get() => _materialTransferRepo.Get();
        async Task<string> IMaterialTransferService.GetIssueIdAsync() => await _materialTransferRepo.GetIssueIdAsync();
        async Task<IEnumerable<object>> IMaterialTransferService.GetMeterialByJobId(int jobId) => await _materialTransferRepo.GetMeterialByJobId(jobId);
        async Task<PagedResponse<object>> IMaterialTransferService.GetMaterialTransferDetailsByDealer(string? searchTerm, string dealerCode, int pageIndex, int pageSize) => await _materialTransferRepo.GetMaterialTransferDetailsByDealer(searchTerm, dealerCode, pageIndex, pageSize);
        async Task<int> IMaterialTransferService.InsertMaterials(List<MaterialTransferViewModel> materialTransferViewModels
            ) => await _materialTransferRepo.InsertMaterials(materialTransferViewModels);
        async Task<int> IMaterialTransferService.DeleteMaterials(List<int> ids) => await _materialTransferRepo.DeleteMaterials(ids);
        async Task<int> IMaterialTransferService.UpdateMaterialDetails(List<MaterialTransferViewModel> materialTransferViewModels) => await _materialTransferRepo.UpdateMaterialDetails(materialTransferViewModels);

        public async Task<byte[]> downloadMaterialExcel(string? dealerCode)
        {
            try
            {
                var data = await _materialTransferRepo.GetMaterialTransferExcelByDealer(dealerCode);

                var properties = typeof(MaterialTransferExcelViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        dict[prop.Name] = prop.GetValue(d);
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "Material Transfer",
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
