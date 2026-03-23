using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PurchaseOrder
{
    public interface IPurchaseOrderService
    {
        Task<bool> CreatePOAsync(PurchaseOrderViewModel model,string userId);
        Task<PurchaseOrderResponseViewModel> GetPOByNumberAsync(string poNumber);
        Task<List<PurchaseOrderResponseViewModel>> GetPOListAsync();
        Task<POERPRequestViewModel> ConvertPOToERPJsonAsync(string poNumber);



    }
}
