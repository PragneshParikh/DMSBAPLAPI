using DMS_BAPL_Utils.ViewModels;
using DMS_BAPL_Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Data.Repositories.VehicleSaleReportRepo;

namespace DMS_BAPL_Data.Services.VehicleSaleReportService
{
    public class VehicleSaleReportService : IVehicleSaleReportService
    {

        private readonly IVehicleSaleReportRepo _vehicleSaleReportRepo;

        public VehicleSaleReportService(IVehicleSaleReportRepo vehicleSaleReportRepo) 
        { 
            _vehicleSaleReportRepo = vehicleSaleReportRepo; 
        }
        public async Task<List<VehicleSaleReportViewModel>> GetVehicleSaleReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            return await _vehicleSaleReportRepo.GetVehicleSaleReportAsync(
               fromDate,
               toDate,
               dealerCode);
        }
    }
}
