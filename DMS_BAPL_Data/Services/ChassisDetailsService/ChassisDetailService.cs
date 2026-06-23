using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ChassisDetailRepo;
using DMS_BAPL_Utils.ViewModels;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ChassisDetailsService
{
    public partial class ChassisDetailService : IChassisDetailService
    {
        private readonly IChassisDetailRepo _chassisDetailRepo;
        public ChassisDetailService(IChassisDetailRepo chassisDetailRepo)
        {
            _chassisDetailRepo = chassisDetailRepo;
        }

        Task<bool> IChassisDetailService.InsertChassis(VehicleInwardViewModel vehicleInward, string userId)
        {
            var chassis = new ChassisDetail
            {
                ChassisNo = vehicleInward.chasis_no,
                ItemCode = vehicleInward.item_code,
                ItemColor = vehicleInward.colr_code,
                DealerId = vehicleInward.dealer_code,
                LocationCode = vehicleInward.loc_code,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            return _chassisDetailRepo.InsertChassis(chassis);
        }

        Task<List<VehicleStockTransferChassisListViewModel>> IChassisDetailService.GetChassisList(string? locationCode) => _chassisDetailRepo.GetChassisList(locationCode);

        async Task<List<ChassisWithRegisterNoViewModel>> IChassisDetailService.GetSoldChassisDetailsList()
        {
            try
            {
                return await _chassisDetailRepo.GetSoldChassisDetailsList();
            }
            catch
            {
                throw;
            }
        }
    }
}