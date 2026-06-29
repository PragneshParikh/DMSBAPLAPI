using DMS_BAPL_Data.Repositories.VehicleInfoRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleInfoService
{
    public class VehicleInfoService:IVehicleInfoService
    {
        private IVehicleInfoRepo _vehicleInfoRepo;
        public VehicleInfoService(IVehicleInfoRepo vehicleInfoRepo)
        {
            _vehicleInfoRepo = vehicleInfoRepo;
        }
        public async Task<VehicleInfoViewModel?> GetVehicleInfoByRegNoChassis(string? regNo, string? chassisNo,string? dealerCode)
        {
            try 
            { 
                return await _vehicleInfoRepo.GetVehicleInfoByRegNoChassis(regNo, chassisNo,dealerCode);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateVehicleInfo(UpdateVehicleInfoViewModel model)
        {
            try
            {
                await _vehicleInfoRepo.UpdateVehicleInfo(model);
            }
            catch
            {
                throw;
            }
        }
    }
}
