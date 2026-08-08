using DMS_BAPL_Utils.ViewModels;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.WarrantyOrderRepo
{
    public interface IWarrantyOrderRepo
    {
        Task<int> InsertWarrantyOrder(WarrantyOrderViewModel model, string userId);
        Task<bool> UpdateWarrantyOrder(WarrantyOrderViewModel model, string userId);
        Task<bool> DeleteWarrantyOrder(int id, string userId);
        Task<WarrantyOrderViewModel?> GetWarrantyOrderById(int id);
        Task<WarrantyOrderSearchResultViewModel> SearchWarrantyOrders(WarrantyOrderSearchViewModel filter);
        Task<NextOrderNumberViewModel> GetNextOrderNumbers(string dealerCode);
        Task<WarrantyJCClaimFullViewModel?> GetWarrantyJCClaimById(int id);

        Task<byte[]> GenerateWarrantyOrderPdf(int id);
    }
}