using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.VehicleDispatchRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleDispatchService
{
    public class VehicleDispatchService : IVehicleDispatchService
    {
        private readonly IVehicleDispatchRepo _vehicleDispatchRepo;

        public VehicleDispatchService(IVehicleDispatchRepo vehicleDispatchRepo)
        {
            _vehicleDispatchRepo = vehicleDispatchRepo;
        }
        Task<IEnumerable<VehicleDispatch>> IVehicleDispatchService.Get()
        {
            return _vehicleDispatchRepo.Get();
        }

        Task<IEnumerable<VehicleDispatch>> IVehicleDispatchService.GetVehicleByStatus(bool status)
        {
            return _vehicleDispatchRepo.GetVehicleByStatus(status);
        }
    }
}
