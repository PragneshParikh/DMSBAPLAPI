using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PartInventoryRepo;
using DMS_BAPL_Data.Repositories.PartInwardRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PartsInwardService
{
    public partial class PartInwardService : IPartInwardService
    {
        private readonly IPartInwardRepo _partInwardRepo;
        private readonly IExcelService _excelService;
        public PartInwardService(IPartInwardRepo partInwardRepo, IExcelService excelService)
        {
            _partInwardRepo = partInwardRepo;
            _excelService = excelService;
        }

        Task<IEnumerable<PartsInward>> IPartInwardService.Get() => _partInwardRepo.Get();
        Task<IEnumerable<PartsInward>> IPartInwardService.GetPartInwardByDealerAsync(string dealerCode) => _partInwardRepo.GetPartInwardByDealerAsync(dealerCode);
        Task<bool> IPartInwardService.UpdateByInvoice(PartsInwardDetailsViewModel partsInwardDetailsViewModel) => _partInwardRepo.UpdateByInvoice(partsInwardDetailsViewModel);
        Task<object> IPartInwardService.PartsInward(PartsInwardViewModel partsInwardViewModel) => _partInwardRepo.PartsInward(partsInwardViewModel);
        Task<IEnumerable<PartsInward>> IPartInwardService.GetPendingPartInwardDetailByLocation(string locationCode) => _partInwardRepo.GetPendingPartInwardDetailByLocation(locationCode);
        Task<object> IPartInwardService.GetInwardPartDetailsByInvoiceNo(string invoiceNo) => _partInwardRepo.GetInwardPartDetailsByInvoiceNo(invoiceNo);
        Task<Object> IPartInwardService.GetPartsInwardDetailsByDealer(int pageIndex, int pageSize, DateTime fromDate, DateTime toDate, string? dealerCode) => _partInwardRepo.GetPartsInwardDetailsByDealer(pageIndex, pageSize, fromDate, toDate, dealerCode);
        public async Task<byte[]> DownloadPartsInwardExcel(DateTime fromDate, DateTime toDate, string? dealerCode)
        {
            try
            {
                var data = await _partInwardRepo.GetPartInwardExcelByDealer(fromDate, toDate, dealerCode);

                // Get all DTO properties for columns
                var properties = typeof(PartInwardExcelViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);

                        if (entityProp != null)
                            dict[prop.Name] = entityProp.GetValue(d);
                        else
                            dict[prop.Name] = null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "PartInwardExcel",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

    }
}
