using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.itemMasterService
{
    public interface IitemMasterService
    {
        Task<insertItemMasterViewModel> InsertItemAsync(insertItemMasterViewModel item, string userId, bool canEditRestrictedFields, bool isDealer);
        Task<List<ItemMasterViewModel>> GetAllItemMastersAsync(int? grpidno, string? search, string? dealerCode = null);
        Task<ItemMaster> UpdateItemAsync(ItemMaster item, bool canEditRestrictedFields, string userId, bool isDealer);
        Task<byte[]> DownloadItemMasterExcel(string? dealerCode = null);
        Task<ItemMasterViewModel> GetPurchaseDetailsByModelNo(string modelNo);
        Task<ItemMasterViewModel> GetPurchaseDetailsWithHsnTaxByModelNo(string modelNo);
        Task<IEnumerable<ItemMaster>> GetItemByItemType(int itemType);
        Task<object> UpdateByItemCode(string userId, insertItemMasterViewModel insertItemMasterViewModel, bool canEditRestrictedFields, bool isDealer);
        Task<IEnumerable<ItemMaster>> GetItemsByOEMModel(int id);
        Task<IEnumerable<object>> GetItemsWithHSNTaxGroupId(int? groupId, string? dealerCode);
        Task<List<ItemPartsByLocationViewModel>> GetItemsByLocation(string dealerLocation, string customerLocation);
        Task<List<ItemMasterViewModel>> GetItemModelist();
        Task<ItemImportResultViewModel> ImportItemExcelAsync(IFormFile file, string userId, bool canEditRestrictedFields, bool isDealer, string? dealerCode = null);
    }
}
