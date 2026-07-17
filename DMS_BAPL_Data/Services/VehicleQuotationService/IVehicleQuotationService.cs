using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleQuotationService
{
    public interface IVehicleQuotationService
    {
        Task<bool> DeleteAsync(long id);
        Task<string> GenerateQuotationNo();
        Task<List<VehicleQuotationViewModel>> GetAllAsync(string? dealerCode = null);
        Task<VehicleQuotationViewModel> GetByIdAsync(long id);
        Task<long> SaveAsync(AddVehicleQuotationViewModel model);
        Task<bool> UpdateAsync(AddVehicleQuotationViewModel model);
    }
}