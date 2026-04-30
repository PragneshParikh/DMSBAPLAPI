using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.itemMasterService
{
    public class ItemMasterService : IitemMasterService
    {

        private readonly IitemMasterRepo _itemMasterRepo;
        private readonly IExcelService _excelService;

        public ItemMasterService(IitemMasterRepo itemMasterRepo, IExcelService excelService)
        {
            _itemMasterRepo = itemMasterRepo;
            _excelService = excelService;
        }

        // add  itemserice to the database

        public async Task<insertItemMasterViewModel> InsertItemAsync(insertItemMasterViewModel item, string userId)
        {
            return await _itemMasterRepo.InsertItemAsync(item, userId);
        }
        // get all itemservice from the database
        public async Task<List<ItemMasterViewModel>> GetAllItemMastersAsync(int? grpidno, string? search)
        {
            return await _itemMasterRepo.GetAllItemsAsync(grpidno, search);
        }

        // update itemservice to the database

        public async Task UpdateItemAsync(ItemMaster item)
        {
            await _itemMasterRepo.UpdateItemAsync(item);
        }

        public async Task<byte[]> DownloadItemMasterExcel()
        {
            try
            {
                var data = await _itemMasterRepo.GetAllExcelItemsAsync();

                // Get all DTO properties for columns
                var properties = typeof(ItemMaster)
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
                    SheetName = StringConstants.DealerExcelSheetName,
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
        /// Get PurchaseDetails By ModelNo 
        /// </summary>
        public async Task<ItemMasterViewModel> GetPurchaseDetailsByModelNo(string modelNo)
        {
            return await _itemMasterRepo.GetPurchaseDetailsByModelNo(modelNo);
        }
        /// <summary>
        /// Get Purchase Details With HsnTax By ModelNo
        /// </summary>
        /// <param name="modelNo"></param>
        /// <returns></returns>
        public async Task<ItemMasterViewModel> GetPurchaseDetailsWithHsnTaxByModelNo(string modelNo)
        {
            return await _itemMasterRepo.GetPurchaseDetailsWithHsnTaxByModelNo(modelNo);
        }

        public Task<IEnumerable<ItemMaster>> GetItemByItemType(int itemType) => _itemMasterRepo.GetItemByItemType(itemType);
        public Task<IEnumerable<ItemMaster>> GetItemsByOEMModel(int id) => _itemMasterRepo.GetItemsByOEMModel(id);

        public Task<object> UpdateByItemCode(string itemCode, string userId, insertItemMasterViewModel insertItemMasterViewModel) => _itemMasterRepo.UpdateByItemCode(itemCode, userId, insertItemMasterViewModel);
    }
}
