using DMS_BAPL_Data.Repositories.VehicleQuotationRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DMS_BAPL_Data.Services.VehicleQuotationService
{
    public class VehicleQuotationService : IVehicleQuotationService
    {
        private readonly IVehicleQuotationRepo _vehicleQuotationRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public VehicleQuotationService(
            IVehicleQuotationRepo vehicleQuotationRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _vehicleQuotationRepo = vehicleQuotationRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<VehicleQuotationViewModel>> GetAllAsync(string? dealerCode = null)
        {
            var isAdmin = GetUserInfoFromToken.GetUserGroup(_httpContextAccessor.HttpContext);

            var effectiveDealerCode = isAdmin
                ? dealerCode
                : GetUserInfoFromToken.GetDealerCodeFromToken(_httpContextAccessor.HttpContext);

            return await _vehicleQuotationRepo.GetAllAsync(effectiveDealerCode);
        }

        public async Task<VehicleQuotationViewModel> GetByIdAsync(long id)
        {
            return await _vehicleQuotationRepo.GetByIdAsync(id);
        }
        public async Task<string> GenerateQuotationNo()
        {
            return await _vehicleQuotationRepo.GenerateQuotationNo();
        }
        public async Task<long> SaveAsync(AddVehicleQuotationViewModel model)
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);
            model.CreatedBy = userId;
            return await _vehicleQuotationRepo.InsertAsync(model);
        }
        public async Task<bool> UpdateAsync(AddVehicleQuotationViewModel model)
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);
            return await _vehicleQuotationRepo.UpdateAsync(model, userId);
        }
        public async Task<bool> DeleteAsync(long id)
        {
            return await _vehicleQuotationRepo.DeleteAsync(id);
        }
    }
}