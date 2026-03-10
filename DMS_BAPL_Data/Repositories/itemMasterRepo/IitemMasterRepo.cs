using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Data;
using DMS_BAPL_Data.DBModels;

namespace DMS_BAPL_Data.Repositories.itemMasterRepo
{
    public interface IitemMasterRepo
    {
        
            Task InsertItemAsync(ItemMaster item);
            Task<List<ItemMaster>> GetAllItemsAsync(int ? grpidno);
            Task UpdateItemAsync(ItemMaster item);

    }
}
