using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleSaleBillService
{
    public interface IVehicleSaleBillService
    {
        Task<int> CreateAsync(VehicleSaleBillEditCreateViewModel model);
        Task<VehicleSaleBillResponseViewModel?> GetByIdAsync(int id);
        //Task<List<VehicleSaleBillResponseViewModel>> GetAllAsync();
        Task<List<VehicleSaleBillResponseViewModel>> GetAllAsync(string? dealerCode, string? search = null, DateTime? dateFrom = null, DateTime? dateTo = null,string? erpStatus =null);
        Task UpdateAsync(int id, VehicleSaleBillEditCreateViewModel model);
        Task DeleteAsync(int id);
        Task<string> GenerateNextVehicleSaleNo();

        //Task<VehicleSaleExportViewModel?> GetExportData(int id);
        // Task<List<VehicleSaleChasisResponse>> GetChasisList(VehicleSaleChasisRequest request);
        Task<List<PdiOkVehicleChassisViewModel>> GetPdiVehiclesAsync(string dealerCode, int ledgerId);
        Task<int> ConfirmInvoiceAndReserveChassis(string saleBillNo);
        Task<VehicleSaleBillHeader> UpdateRegistrationAndReserveChassis(string? saleBillNo, List<UpdateSaleDetailsVM> updateSaleDetails);
        
        Task<byte[]> DownloadSaleBillExcel(DateTime? dateFrom = null, DateTime? dateTo = null);

        Task<IEnumerable<string>> GetPolicyNo(string chassisNo);
        Task<List<ChassisListWithPDIStatus>> GetAllChassissListWithPDISatatus(string? dealerCode,int ledgerId);
        Task<VehicleSaleExportViewModel?> GetExportDetails(string dealerCode, int id);
    }
}
