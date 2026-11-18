using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.VehicleDispatchRepo;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleDispatchService
{
    public class VehicleInwardService : IVehicleInwardService
    {
        private readonly IVehicleInwardRepo _vehicleDispatchRepo;

        public VehicleInwardService(IVehicleInwardRepo vehicleDispatchRepo)
        {
            _vehicleDispatchRepo = vehicleDispatchRepo;
        }
        Task<IEnumerable<VehicleInward>> IVehicleInwardService.Get()
        {
            return _vehicleDispatchRepo.Get();
        }

        Task<IEnumerable<VehicleInwardD2DViewModel>>IVehicleInwardService.GetVehicleByStatus(string dealerCode, bool status)
        {
            return _vehicleDispatchRepo.GetVehicleByStatus(dealerCode, status);
        }

        Task<bool> IVehicleInwardService.UpdateInvoiceStatus(string invoiceNo, string userId)
        {
            return _vehicleDispatchRepo.UpdateInvoiceStatus(invoiceNo, userId);
        }

        Task<object> IVehicleInwardService.InsertVehicleInwardDetail(VehicleInwardViewModel vehicleInwardViewModel) => _vehicleDispatchRepo.InsertVehicleDispatchDetail(vehicleInwardViewModel);
        Task<ApiResponse> IVehicleInwardService.DeleteInvoice(string invoiceNo, string userId)
        {
            return _vehicleDispatchRepo.DeleteInvoiceAsync(invoiceNo, userId);
        }
        Task<ApiResponse> IVehicleInwardService.DeleteVehicles(List<string> chassisNos, string userId)
        {
            return _vehicleDispatchRepo.DeleteVehiclesAsync(chassisNos, userId);
        }
    }
}
