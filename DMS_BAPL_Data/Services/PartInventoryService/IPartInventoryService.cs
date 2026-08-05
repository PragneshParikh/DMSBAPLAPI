using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
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
        Task<IEnumerable<object>> Get();
        Task<IEnumerable<object>> GetByItemCode(List<string> itemCode);
        Task<IEnumerable<object>> GetPartsByDealerAndDateRange(InventoryFilterViewModel inventoryFilterViewModel);
    }
}
