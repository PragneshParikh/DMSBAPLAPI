using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleQuotationRepo
{
    public interface IVehicleQuotationRepo
    {
        Task<bool> DeleteAsync(long id);
        Task<string> GenerateQuotationNo();
        Task<List<VehicleQuotationViewModel>> GetAllAsync(string? dealerCode = null);
        Task<VehicleQuotationViewModel> GetByIdAsync(long id);
        Task<long> InsertAsync(AddVehicleQuotationViewModel model);

        // userId is passed separately because AddVehicleQuotationViewModel has no UpdatedBy field.
        Task<bool> UpdateAsync(AddVehicleQuotationViewModel model, string userId);

        Task<VehicleQuotationPrintViewModel> GetPrintQuotationAsync(long quotationId);
    }
}