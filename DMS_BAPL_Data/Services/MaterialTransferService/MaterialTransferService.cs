using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.JobCardRepo;
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

        // NEW — needed so that DeleteMaterialsByJobId can flip
        // JobCardHeader.IsMaterialTransfer back to false once the
        // material transfer rows for a job are gone. Previously this
        // reset only happened inside RepairBillRepo.DeleteRepairbill as
        // a side effect of deleting a repair bill — which was wrong,
        // because it fired even when the repair bill delete should have
        // left MT status alone (Condition 1). Now the reset lives here,
        // directly on the action that actually removes the material
        // transfer, so it fires exactly when it should (Condition 2).
        private readonly IJobCardRepo _jobCardRepo;

        public MaterialTransferService(
            IMaterialTransferRepo materialTransferRepo,
            IExcelService excelService,
            IPartInventoryService partInventoryService,
            IJobCardRepo jobCardRepo)
        {
            _materialTransferRepo = materialTransferRepo;
            _excelService = excelService;
            _partInventoryService = partInventoryService;
            _jobCardRepo = jobCardRepo;
        }

        Task<object> IMaterialTransferService.Get() => _materialTransferRepo.Get();
        async Task<string> IMaterialTransferService.GetIssueIdAsync() => await _materialTransferRepo.GetIssueIdAsync();
        async Task<IEnumerable<object>> IMaterialTransferService.GetMeterialByJobId(int jobId) => await _materialTransferRepo.GetMeterialByJobId(jobId);
        Task<IEnumerable<MaterialTransferViewModel>> IMaterialTransferService.GetMeterialTransferByJobId(int JobId) => _materialTransferRepo.GetMeterialTransferByJobId(JobId);
        async Task<PagedResponse<object>> IMaterialTransferService.GetMaterialTransferDetailsByDealer(string? searchTerm, string? dealerCode, int pageIndex, int pageSize, DateTime? fromDate, DateTime? toDate)
        => await _materialTransferRepo.GetMaterialTransferDetailsByDealer(searchTerm, dealerCode, pageIndex, pageSize, fromDate, toDate);

        async Task<int> IMaterialTransferService.InsertMaterials(List<MaterialTransferViewModel> materialTransferViewModels)
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

        async Task<int> IMaterialTransferService.UpdateMaterialDetails(List<MaterialTransferViewModel> materialTransferViewModels)
        {
            var existingItems = await _materialTransferRepo.GetMeterialTransferByJobId(materialTransferViewModels[0].JobId);

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

            // Handle added/updated items
            foreach (var item in groupedItems)
            {
                var existing = existingItems.FirstOrDefault(x => x.ItemCode == item.ItemCode);

                int oldQty = existing?.Quantity ?? 0;
                int newQty = item.Qty;

                int difference = newQty - oldQty;

                if (difference == 0)
                    continue;

                var stockTransaction = new PartsInventory
                {
                    TransId = Guid.NewGuid().ToString(),
                    ItemCode = item.ItemCode,
                    VoucherNo = null!,
                    BatchNo = "Batch 1",
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

                if (difference > 0)
                {
                    // Consume additional stock
                    stockTransaction.TransType = "S";
                    stockTransaction.BatchTransQty = difference;

                    await _partInventoryService.UpdateOutgoing(stockTransaction);
                }
                else
                {
                    // Return stock
                    stockTransaction.TransType = "PI";
                    stockTransaction.BatchTransQty = Math.Abs(difference);

                    await _partInventoryService.UpdateIncoming(stockTransaction);
                }
            }

            // Handle deleted items
            foreach (var existing in existingItems)
            {
                if (!groupedItems.Any(x => x.ItemCode == existing.ItemCode))
                {
                    var stockTransaction = new PartsInventory
                    {
                        TransId = Guid.NewGuid().ToString(),
                        ItemCode = existing.ItemCode,
                        VoucherNo = null!,
                        TransType = "PO",
                        BatchNo = "Batch 1",
                        BatchTransQty = existing.Quantity,
                        BatchOpeningQty = 0,
                        BatchClosingQty = 0,
                        TransDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    await _partInventoryService.UpdateIncoming(stockTransaction);
                }
            }

            return await _materialTransferRepo.UpdateMaterialDetails(materialTransferViewModels);
        }

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

        async Task<MaterialTransferDeleteResultViewModel> IMaterialTransferService.DeleteMaterialsByJobId(int jobId, bool isSuperAdmin, string createdBy)
        {
            var existingItems = await _materialTransferRepo.GetMeterialTransferByJobId(jobId);

            var groupedItems = existingItems
                .GroupBy(x => x.ItemCode)
                .Select(g => new
                {
                    ItemCode = g.Key,
                    Qty = g.Sum(x => x.Quantity),
                    DealerLocation = g.First().DealerLocation,
                    DealerCode = g.First().DealerCode
                });
            var reversedItems = new List<ReversedStockItemViewModel>();

            foreach (var item in groupedItems)
            {
                var stockTransaction = new PartsInventory
                {
                    TransId = Guid.NewGuid().ToString(),
                    ItemCode = item.ItemCode,
                    VoucherNo = null!,
                    TransType = "SD",
                    BatchNo = "Batch 1",
                    BatchTransQty = item.Qty,
                    BatchOpeningQty = 0,
                    BatchClosingQty = 0,
                    TransDate = DateOnly.FromDateTime(DateTime.Now),
                    DealerLocation = item.DealerLocation,
                    VendorCode = item.DealerCode,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };

                await _partInventoryService.UpdateIncoming(stockTransaction);

                reversedItems.Add(new ReversedStockItemViewModel
                {
                    ItemCode = item.ItemCode,
                    Quantity = item.Qty,
                    DealerCode = item.DealerCode,
                    DealerLocation = item.DealerLocation
                });
            }

            var deletedCount = await _materialTransferRepo.DeleteMaterialsByJobId(jobId, isSuperAdmin);
            if (deletedCount > 0)
            {
                await _jobCardRepo.UpdateMaterialTransferStatus(jobId, false);
            }

            return new MaterialTransferDeleteResultViewModel
            {
                DeletedCount = deletedCount,
                ReversedItems = reversedItems
            };
        }
    }
}