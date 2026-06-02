using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleSaleBillRepo
{
    public interface IVehicleSaleBillRepo
    {
        Task<int> CreateWithJobUpdateAsync(VehicleSaleBillHeader header);
        // Task<int> CreateAsync(VehicleSaleBillHeader entity);
        Task<VehicleSaleBillResponseViewModel?> GetVehicleWithMotorDetailsByIdAsync(int id);
        Task<VehicleSaleBillHeader?> GetByIdAsync(int id);
        Task<List<VehicleSaleBillHeader>> GetAllAsync();
        Task UpdateAsync(VehicleSaleBillHeader entity);
        Task DeleteAsync(int id);
        Task<string?> GetLastSaleBillNo();

        Task<List<PdiOkVehicleChassisViewModel>> GetPdiRawDataAsync(string dealerCode);
        Task<int> UpdateERPStatus(int id);
        Task UpdateWithJobUpdateAsync(VehicleSaleBillHeader header, List<UpdateSaleDetailsVM>? jobUpdates,List<string> deletedChassisList);
        Task<int> ConfirmInvoiceAndReserveChassis(string saleBillNo);
        Task<VehicleSaleBillHeader> UpdateRegistrationAndReserveChassis(string? saleBillNo, List<UpdateSaleDetailsVM> updateSaleDetails);
        Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo);

        Task<IEnumerable<string>> GetPolicyNo(string chassisNo);
        Task<List<ChassisListWithPDIStatus>> GetAllChassissListWithPDISatatus(string? dealerCode);
        Task<VehicleSaleExportViewModel?> GetExportDetails(string dealerCode, int id);
    }
}
