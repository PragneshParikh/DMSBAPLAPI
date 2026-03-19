using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.itemMasterService
{
    public interface IitemMasterService
    {

        Task<ItemMasterViewModel> InsertItemAsync(ItemMasterViewModel item);
        Task<List<ItemMasterViewModel>> GetAllItemMastersAsync(int? grpidno, string? search);

        Task UpdateItemAsync(ItemMaster item);

        Task<byte[]> DownloadItemMasterExcel();
    }
}
