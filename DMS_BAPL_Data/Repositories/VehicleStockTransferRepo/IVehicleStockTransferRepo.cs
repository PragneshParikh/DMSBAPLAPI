using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleStockTransferRepo
{
    public interface IVehicleStockTransferRepo
    {
        Task<int> CreateAsync(VehicleStockTransferCreateEditViewModel model);
        Task<List<VehicleStockTransferListVewModel>> GetVehicleStockTransfer(VehicleStockTransferFilterViewModel filter);
        Task<VehicleStockTransferListVewModel> GetVehicleTransferById(int id);
        Task<List<VehicleStockExcelViewModel>> GetExcelReportData(DateTime? dateFrom, DateTime? dateTo, string? issuingLocation, string? receivingLocation, string? search);
    }
}
