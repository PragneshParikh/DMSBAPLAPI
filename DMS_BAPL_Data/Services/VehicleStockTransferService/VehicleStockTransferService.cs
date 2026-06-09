using DMS_BAPL_Data.Repositories.PrefixRepo;
using DMS_BAPL_Data.Repositories.VehicleStockTransferRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleStockTransferService
{
    public class VehicleStockTransferService : IVehicleStockTransferService
    {
        private readonly IVehicleStockTransferRepo _transferRepo;
        private readonly IExcelService _excelService;
        private readonly IPrefixRepo _prefixRepo;
        public VehicleStockTransferService(IVehicleStockTransferRepo transferRepo, IPrefixRepo prefixRepo, IExcelService excelService)
        {
            _transferRepo = transferRepo;
            _prefixRepo = prefixRepo;
            _excelService = excelService;
        }

        public async Task<int> CreateAsync(VehicleStockTransferCreateEditViewModel model)
        {
            try
            {
                var result = await _transferRepo.CreateAsync(model);
                if (result != 0)
                {
                    await _prefixRepo.UpdateNextNumberByDealerByModule(model.DealerCode, "vehicle_transfer");
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<VehicleStockTransferListVewModel>> GetVehicleStockTransfer(VehicleStockTransferFilterViewModel filter)
        {
            try
            {
                return await _transferRepo.GetVehicleStockTransfer(filter);
            }
            catch
            {
                throw;
            }
        }

        public async Task<VehicleStockTransferListVewModel> GetVehicleTransferById(int id)
        {
            try
            {
                return await _transferRepo.GetVehicleTransferById(id);
            }
            catch
            {
                throw;
            }
        }
        public async Task<byte[]> DownloadTransferExcel(DateTime? dateFrom = null, DateTime? dateTo = null, string? issuingLocation = null, string? receivingLocation = null, string? search = null)
        {
            try
            {
                var data = await _transferRepo.GetExcelReportData(dateFrom, dateTo, issuingLocation, receivingLocation, search);
                var properties = typeof(VehicleStockExcelViewModel).GetProperties().ToList();
                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(d);
                        if (value is DateTime dateTime)
                        {
                            dict[prop.Name] = dateTime.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            dict[prop.Name] = value ?? "";
                        }
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "Vehicle Stock Transfer",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

    }
}
