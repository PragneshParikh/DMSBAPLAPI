using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.itemMasterService
{
    public interface IitemMasterService
    {

        Task InsertItemMasterAsync(ItemMaster itemMaster);

        Task<List<ItemMaster>> GetAllItemMastersAsync(int ? grpidno);

        Task UpdateItemAsync(ItemMaster item);
    }
}
