using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PartInventoryRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DMS_BAPL_Data.Services.InventoryService
{
    public partial class PartInventoryService : IPartInventoryService
    {
        private readonly IPartInventoryRepo _partInventoryRepo;

        public PartInventoryService(IPartInventoryRepo partInventoryRepo)
        {
            _partInventoryRepo = partInventoryRepo;
        }

        async Task<int> IPartInventoryService.GetCurrentStockByItem(string itemCode)
        {
            return await _partInventoryRepo.GetCurrentStockByItem(itemCode);
        }

        async Task IPartInventoryService.UpdateIncoming(PartsInventory partsInventory)
        {
            await _partInventoryRepo.UpdateStock(partsInventory);
        }

        async Task IPartInventoryService.UpdateOutgoing(PartsInventory partsInventory)
        {
            await _partInventoryRepo.UpdateStock(partsInventory);
        }

        Task<IEnumerable<object>> IPartInventoryService.Get() => _partInventoryRepo.Get();
        Task<IEnumerable<object>> IPartInventoryService.GetByItemCode(List<string> itemCode) => _partInventoryRepo.GetByItemCode(itemCode);
        Task<IEnumerable<object>> IPartInventoryService.GetPartsByDealerAndDateRange(InventoryFilterViewModel inventoryFilterViewModel)
         => _partInventoryRepo.GetPartsByDealerAndDateRange(inventoryFilterViewModel);
    }
}
