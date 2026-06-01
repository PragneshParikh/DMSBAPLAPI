using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ChassisBatteryDetailRepo;
using DMS_BAPL_Utils.ViewModels;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ChassisBatteryDetailService
{
    public partial class ChassisBatteryDetailService : IChassisBatteryDetailService
    {
        private readonly IChassisBatteryDetailRepo _chassisBatteryDetailRepo;

        public ChassisBatteryDetailService(IChassisBatteryDetailRepo chassisBatteryDetailRepo)
        {
            _chassisBatteryDetailRepo = chassisBatteryDetailRepo;
        }

        Task<bool> IChassisBatteryDetailService.InsertBatteryDetail(VehicleInwardViewModel vehicleInward, string userId)
        {
            var batteryDetail = new ChassisBatteryDetail
            {
                ChassisNo = vehicleInward.chasis_no,
                MotorNo = vehicleInward?.motor_no,
                BatteryNo = vehicleInward.battery_no,
                ChargerNo = vehicleInward.charger_no,
                ControllerNo = vehicleInward.controller_no,
                BatteryCapacity = vehicleInward.battery_capacity,
                BatteryMake = vehicleInward.battery_make,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            return _chassisBatteryDetailRepo.InsertBatteryDetail(batteryDetail);
        }
    }
}
