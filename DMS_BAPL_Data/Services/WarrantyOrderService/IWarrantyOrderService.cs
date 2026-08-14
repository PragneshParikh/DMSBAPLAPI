using DMS_BAPL_Utils.ViewModels;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.WarrantyOrderService
{
    public interface IWarrantyOrderService
    {
        Task<int> CreateWarrantyOrder(WarrantyOrderViewModel model, string userId);
        Task<bool> UpdateWarrantyOrder(WarrantyOrderViewModel model, string userId);
        Task<bool> DeleteWarrantyOrder(int id, string userId);
        Task<WarrantyOrderViewModel?> GetWarrantyOrderById(int id);
        Task<WarrantyOrderSearchResultViewModel> SearchWarrantyOrders(WarrantyOrderSearchViewModel filter);
        Task<NextOrderNumberViewModel> GetNextOrderNumbers(string dealerCode);
        Task<WarrantyJCClaimFullViewModel?> GetWarrantyJCClaimById(int id);

        Task<byte[]> GenerateWarrantyOrderPdf(int id);

        Task<List<string>> SearchBatchNos(string dealerCode, string searchText);
        Task<List<string>> SearchOrderNos(string dealerCode, string searchText);
        Task<List<LocationDropdownItemViewModel>> GetDistinctOrderLocations(string dealerCode);
    }
}