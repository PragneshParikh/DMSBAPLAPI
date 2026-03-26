using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.VehicleDispatchRepo;
using DocumentFormat.OpenXml.Packaging;
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

        Task<IEnumerable<VehicleDispatch>> IVehicleDispatchService.GetVehicleByStatus(string dealerCode, bool status)
        {
            return _vehicleDispatchRepo.GetVehicleByStatus(dealerCode, status);
        }

        Task<bool> IVehicleDispatchService.UpdateInvoiceStatus(string invoiceNo, string userId)
        {
            return _vehicleDispatchRepo.UpdateInvoiceStatus(invoiceNo, userId);
        }

        Task<bool> IVehicleDispatchService.InsertVehicleDispatchDetail(List<VehicleDispatch> vehicleDispatches) => _vehicleDispatchRepo.InsertVehicleDispatchDetail(vehicleDispatches);

    }
}
