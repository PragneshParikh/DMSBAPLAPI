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
        Task UpdateItemAsync(ItemMaster item);
        Task<ItemMaster> GetItemByCodeAsync(string itemCode);
        Task<ItemMasterViewModel> GetPurchaseDetailsByModelNo(string modelNo);
        Task<IEnumerable<ItemMaster>> GetItemByItemType(int itemType);
    }
}
