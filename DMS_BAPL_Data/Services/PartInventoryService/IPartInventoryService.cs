using DMS_BAPL_Data.DBModels;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.InventoryService
{
    public interface IPartInventoryService
    {
        Task UpdateIncoming(PartsInventory partsInventory);
        Task UpdateOutgoing(PartsInventory partsInventory);
        Task<int> GetCurrentStockByItem(string itemCode);
    }
}
