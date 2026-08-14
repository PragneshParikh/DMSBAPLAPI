using DMS_BAPL_Data.Repositories.WarrantyOrderRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.WarrantyOrderService
{
    public class WarrantyOrderService : IWarrantyOrderService
    {
        private readonly IWarrantyOrderRepo _warrantyOrderRepo;

        public WarrantyOrderService(IWarrantyOrderRepo warrantyOrderRepo)
        {
            _warrantyOrderRepo = warrantyOrderRepo;
        }

        public async Task<int> CreateWarrantyOrder(WarrantyOrderViewModel model, string userId)
        {
            // Business rule: a batch order must actually group at least one claim.
            if (model.WarrantyClaimIds == null || !model.WarrantyClaimIds.Any())
                throw new InvalidOperationException("At least one Warranty Claim must be selected to create an Order.");

            return await _warrantyOrderRepo.InsertWarrantyOrder(model, userId);
        }

        public async Task<bool> UpdateWarrantyOrder(WarrantyOrderViewModel model, string userId)
        {
            if (model.Id <= 0)
                throw new InvalidOperationException("A valid Warranty Order Id is required for update.");

            if (model.WarrantyClaimIds == null || !model.WarrantyClaimIds.Any())
                throw new InvalidOperationException("At least one Warranty Claim must be selected for the Order.");

            return await _warrantyOrderRepo.UpdateWarrantyOrder(model, userId);
        }

        public async Task<bool> DeleteWarrantyOrder(int id, string userId)
        {
            return await _warrantyOrderRepo.DeleteWarrantyOrder(id, userId);
        }

        public async Task<WarrantyOrderViewModel?> GetWarrantyOrderById(int id)
        {
            return await _warrantyOrderRepo.GetWarrantyOrderById(id);
        }

        public async Task<WarrantyOrderSearchResultViewModel> SearchWarrantyOrders(WarrantyOrderSearchViewModel filter)
        {
            return await _warrantyOrderRepo.SearchWarrantyOrders(filter);
        }

        public async Task<NextOrderNumberViewModel> GetNextOrderNumbers(string dealerCode)
        {
            return await _warrantyOrderRepo.GetNextOrderNumbers(dealerCode);
        }

        public async Task<WarrantyJCClaimFullViewModel?> GetWarrantyJCClaimById(int id)
        {
            return await _warrantyOrderRepo.GetWarrantyJCClaimById(id);
        }

        public async Task<byte[]> GenerateWarrantyOrderPdf(int id)
        {
            return await _warrantyOrderRepo.GenerateWarrantyOrderPdf(id);
        }

        public async Task<List<string>> SearchBatchNos(string dealerCode, string searchText)
        {
            return await _warrantyOrderRepo.SearchBatchNos(dealerCode, searchText);
        }

        public async Task<List<string>> SearchOrderNos(string dealerCode, string searchText)
        {
            return await _warrantyOrderRepo.SearchOrderNos(dealerCode, searchText);
        }

        public async Task<List<LocationDropdownItemViewModel>> GetDistinctOrderLocations(string dealerCode)
        {
            return await _warrantyOrderRepo.GetDistinctOrderLocations(dealerCode);
        }
    }
}