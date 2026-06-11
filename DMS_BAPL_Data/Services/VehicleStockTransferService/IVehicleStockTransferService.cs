using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleStockTransferService
{
    public interface IVehicleStockTransferService
    {
        Task<int> CreateAsync(VehicleStockTransferCreateEditViewModel model);
        Task<List<VehicleStockTransferListVewModel>> GetVehicleStockTransfer(VehicleStockTransferFilterViewModel filter);
        Task<VehicleStockTransferListVewModel> GetVehicleTransferById(int id);
        Task<byte[]> DownloadTransferExcel(DateTime? dateFrom = null, DateTime? dateTo = null, string? issuingLocation = null, string? receivingLocation = null, string? search = null);
    }
}
