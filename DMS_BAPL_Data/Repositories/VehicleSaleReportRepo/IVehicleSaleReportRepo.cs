using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleSaleReportRepo
{
    public interface IVehicleSaleReportRepo
    {
        Task<List<VehicleSaleReportViewModel>> GetVehicleSaleReportAsync(
             DateTime? fromDate,
             DateTime? toDate,
             string? dealerCode);
    }
}
