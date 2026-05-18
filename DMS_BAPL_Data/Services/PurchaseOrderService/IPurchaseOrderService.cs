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
        Task<bool> CreatePOAsync(PurchaseOrderViewModel model, string dealerCode);
        Task<PurchaseOrderResponseViewModel> GetPOByNumberAsync(string poNumber);
        Task<List<PurchaseOrderResponseViewModel>> GetPOListAsync(string? dealerCode);
        Task<bool> CreatePartsPOAsync(PartsPurchaseOrderViewModel model, string dealerCode);
        Task<List<PartsPurchaseOrderResponseViewModel>> GetPartsPOListAsync();
        Task<POERPRequestViewModel> ConvertPOToERPJsonAsync(string poNumber);
        Task<bool> UpdatePOAsync(PurchaseOrderViewModel model, string dealerCode);
        Task<bool> DeletePOItemsAsync(string poNumber);
        Task<decimal> GetSubsidyValueAsync();
        Task<byte[]> DownloadPurchaseOrderExcel(PurchaseOrderSearchViewModel filter);
        Task<bool> UpdatePOStatusAsync(UpdatePOStatusViewModel updatePOStatusViewModel);
    }
}
