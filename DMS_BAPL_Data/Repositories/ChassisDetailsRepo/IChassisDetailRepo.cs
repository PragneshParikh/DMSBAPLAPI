using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ChassisDetailsRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ChassisDetailRepo
{
    public interface IChassisDetailRepo
    {
        Task<bool> InsertChassis(ChassisDetail chassisDetail);
        Task<List<VehicleStockTransferChassisListViewModel>> GetChassisList(string? locationCode);
        Task<List<ChassisWithRegisterNoViewModel>> GetSoldChassisDetailsList();
        Task<bool> UpdateNewLedgerForChassis(int ledgerId, string dealerCode, string chassisNo, string userId);
    }
}
