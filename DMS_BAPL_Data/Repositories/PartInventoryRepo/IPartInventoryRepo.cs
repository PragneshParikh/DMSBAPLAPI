using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PartInventoryRepo
{
    public interface IPartInventoryRepo
    {
        Task<int> GetCurrentStockByItem(string itemCode);
        Task UpdateStock(string itemCode, int quantity, string trasactionType);
    }
}
