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
        Task<VehicleSaleBillHeader?> GetByIdAsync(int id);
        Task<List<VehicleSaleBillHeader>> GetAllAsync();
        Task UpdateAsync(VehicleSaleBillHeader entity);
        Task DeleteAsync(int id);
        Task<string?> GetLastSaleBillNo();

        Task<string?> GetDealerLocation(string dealerCode);
        Task<ItemMaster?> GetItem(string itemCode);
        Task<decimal?> GetPurchaseRate(string dealerCode, string itemCode);
        Task<List<(string chassisNo, string itemCode)>> GetChassisByDealer(string dealerCode);
        Task<List<PdiOkVehicleChassisViewModel>> GetPdiRawDataAsync(string dealerCode);
        Task<int> UpdateERPStatus(int id);
        Task UpdateWithJobUpdateAsync(VehicleSaleBillHeader header, List<UpdateSaleDetailsVM>? jobUpdates,List<string> deletedChassisList);
        Task<bool> ConfirmInvoiceAndReserveChassis(string saleBillNo);
        Task<VehicleSaleBillHeader> UpdateRegistrationAndReserveChassis(string? saleBillNo, List<UpdateSaleDetailsVM> updateSaleDetails);
    }
}
