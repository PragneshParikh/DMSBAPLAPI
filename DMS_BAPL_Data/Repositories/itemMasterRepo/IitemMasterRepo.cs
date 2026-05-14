using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Data;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.itemMasterRepo
{
    public interface IitemMasterRepo
    {
        Task<insertItemMasterViewModel> InsertItemAsync(insertItemMasterViewModel item, string userId);
        Task<List<ItemMasterViewModel>> GetAllItemsAsync(int? grpidno, string? search);
        Task<List<ItemMaster>> GetAllExcelItemsAsync();
        Task<ItemMaster> UpdateItemAsync(ItemMaster item);
        Task<ItemMaster> GetItemByCodeAsync(string itemCode);
        Task<ItemMasterViewModel> GetPurchaseDetailsByModelNo(string modelNo);
        Task<ItemMasterViewModel> GetPurchaseDetailsWithHsnTaxByModelNo(string modelNo);
        Task<IEnumerable<ItemMaster>> GetItemByItemType(int itemType);
        Task<object> UpdateByItemCode(string userId, insertItemMasterViewModel insertItemMasterViewModel);
        Task<IEnumerable<ItemMaster>> GetItemsByOEMModel(int id);
        Task<List<ItemMaster>> GetByItemCodesAsync(List<string> itemCodes);
        Task<IEnumerable<object>> GetItemsWithHSNTaxGroupId(int? groupId);
    }
}
