using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.itemMasterService
{
    public class ItemMasterService : IitemMasterService
    {

        private readonly IitemMasterRepo _itemMasterRepo;

        public ItemMasterService(IitemMasterRepo itemMasterRepo)
        {
            _itemMasterRepo = itemMasterRepo;
        }

        // add  itemserice to the database
        public async Task InsertItemMasterAsync(ItemMaster itemMaster)
        {
            await _itemMasterRepo.InsertItemAsync(itemMaster);
        }

        // get all itemservice from the database
        public async Task<List<ItemMaster>> GetAllItemMastersAsync(int ? grpidno)
        {
            return await _itemMasterRepo.GetAllItemsAsync(grpidno);
        }

        // update itemservice to the database

        public async Task UpdateItemAsync(ItemMaster item)
        {
            await _itemMasterRepo.UpdateItemAsync(item);
        }
    }
}
