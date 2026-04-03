using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.InventoryService
{
    public interface IPartInventoryService
    {
        Task UpdateIncoming(string itemCode, int quantity);
        Task UpdateOutgoing(string itemCode, int quantity);
        Task<int> GetCurrentStockByItem(string itemCode);
    }
}
