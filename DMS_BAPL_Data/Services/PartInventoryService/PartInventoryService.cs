using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PartInventoryRepo;
using DMS_BAPL_Utils.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;
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

    }
}
