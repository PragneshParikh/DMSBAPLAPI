using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.MaterialTransferRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
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
        private readonly IPartInventoryService _partInventoryService;

        public MaterialTransferService(IMaterialTransferRepo materialTransferRepo, IExcelService excelService, IPartInventoryService partInventoryService)
        {
            _materialTransferRepo = materialTransferRepo;
            _excelService = excelService;
            _partInventoryService = partInventoryService;
        }

        Task<object> IMaterialTransferService.Get() => _materialTransferRepo.Get();
        async Task<string> IMaterialTransferService.GetIssueIdAsync() => await _materialTransferRepo.GetIssueIdAsync();
        async Task<IEnumerable<object>> IMaterialTransferService.GetMeterialByJobId(int jobId) => await _materialTransferRepo.GetMeterialByJobId(jobId);
        async Task<PagedResponse<object>> IMaterialTransferService.GetMaterialTransferDetailsByDealer(string? searchTerm, string dealerCode, int pageIndex, int pageSize) => await _materialTransferRepo.GetMaterialTransferDetailsByDealer(searchTerm, dealerCode, pageIndex, pageSize);
        async Task<int> IMaterialTransferService.InsertMaterials(List<MaterialTransferViewModel> materialTransferViewModels
            )
        {

            var groupedItems = materialTransferViewModels
                .GroupBy(x => x.ItemCode)
                .Select(g => new
                {
                    ItemCode = g.Key,
                    Qty = g.Sum(x => x.Quantity),
                    Location = g.First().Location,
                    DealerCode = g.First().DealerCode,
                    CreatedBy = g.First().CreatedBy,
                    CreatedDate = g.First().CreatedDate
                })
                .ToList();

            foreach (var item in groupedItems)
            {
                var stockTransaction = new PartsInventory
                {
                    TransId = Guid.NewGuid().ToString(),
                    ItemCode = item.ItemCode,
                    VoucherNo = null!,
                    TransType = "S",
                    BatchNo = "Batch 1",
                    BatchTransQty = item.Qty,
                    BatchOpeningQty = 0,
                    BatchClosingQty = 0,
                    TransDate = DateOnly.FromDateTime(DateTime.Now),
                    DealerLocation = item.Location,
                    VendorCode = item.DealerCode,
                    TotalRate = 100.00M,
                    PurchaseRate = 110.00M,
                    Potype = "B2C",
                    PostTransaction = 0,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate
                };

                await _partInventoryService.UpdateOutgoing(stockTransaction);
            }
            return await _materialTransferRepo.InsertMaterials(materialTransferViewModels);
        }

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
