using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ChassisDetailRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ChassisDetailsService
{
    public interface IChassisDetailService
    {
        Task<bool> InsertChassis(VehicleInwardViewModel vehicleInward, string userId);
        Task<List<VehicleStockTransferChassisListViewModel>> GetChassisList(string? locationCode);
        Task<List<ChassisWithRegisterNoViewModel>> GetSoldChassisDetailsList();
        Task<bool> UpdateNewLedgerForChassis(int ledgerId, string dealerCode, string chassisNo);
    }
}
