using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.HSRPRepo;
using DMS_BAPL_Data.Repositories.PrefixRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.HSRPService
{
    public class HSRPService : IHSRPService
    {
        private readonly IHSRPRepo _hsrpRepo;
        private readonly IPrefixRepo _prefixRepo;
        private readonly IExcelService _excelService;
        public HSRPService(IHSRPRepo hsrpRepo, IPrefixRepo prefixRepo, IExcelService excelService)
        {
            _hsrpRepo = hsrpRepo;
            _prefixRepo = prefixRepo;
            _excelService = excelService;
        }

        public async Task<List<VehicleSaleBillResponseViewModel>> GetAllInvoicedVehicleForHSRPOrder(string? dealerCode)
        {
            try
            {
                return await _hsrpRepo.GetAllInvoicedVehicleForHSRPOrder(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<HSRPOrderAddEditViewModel>> GetPendingHSRPListAsync(string? dealerCode, DateTime? fromdate, DateTime? toDate)
        {
            try
            {
                return await _hsrpRepo.GetPendingHSRPListAsync(dealerCode, fromdate, toDate);
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> ReceiveDispatchAsync(List<HSRPDispatchItem> request)
        {
            return await _hsrpRepo.ReceiveDispatchAsync(request);
        }
        public async Task<List<Hsrporder>> CreateBulkHSRPOrder(
            List<HSRPOrderCreateViwModel> orders)
        {
            try
            {
                var token = await _hsrpRepo.GetHSRPLoginTokenAsync();

                var result = await _hsrpRepo.CreateBulkHSRPOrder(orders, token);

                if (result.Count > 0)
                {
                    await _prefixRepo.UpdateNextNumberByDealerByModule(orders[0].DealerCode, "hsrp_order");
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<HSRPListViewModel>> GetAllHSRPOrderAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                return await _hsrpRepo.GetAllHSRPOrderAsync(dealerCode, fromDate, toDate);
            }
            catch
            {
                throw;
            }
        }

        public async Task<HSRPOrderAddEditViewModel> GetAllHSRPOrderByIdAsync(int id)
        {
            try
            {
                return await _hsrpRepo.GetHSRPOrderByIdAsync(id);
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<Hsrporder>> UpdateBulkHSRPOrder(List<HSRPOrderCreateViwModel> orders)
        //{
        //    try
        //    {
        //        return await _hsrpRepo.UpdateBulkHSRPOrder(orders);
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public async Task<List<HSRPInward>> GetAllHSRPInward(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                return await _hsrpRepo.GetAllHSRPInward(dealerCode, fromDate, toDate);
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<Hsrporder>> UpdateInwardStatus(List<HSRPInwardUpdate> orders)
        //{
        //    try
        //    {
        //        return await _hsrpRepo.UpdateInwardStatus(orders);
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        public async Task<List<Hsrporder>> UpdateInwardStatus(List<HSRPInwardUpdate> orders)
        {
            var token = await _hsrpRepo.GetHSRPLoginTokenAsync();

            return await _hsrpRepo.UpdateInwardStatus(orders, token);
        }
        public async Task<byte[]> DownloadHSRPExcel(bool isSuperAdmin, string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await _hsrpRepo.GetHSRPOrderForExcel(isSuperAdmin, dealerCode, fromDate, toDate);

                var properties = typeof(HSRPExcelViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);
                        dict[prop.Name] = entityProp != null
                            ? entityProp.GetValue(d)
                            : null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.HSRPSheet,
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

        public async Task<HSRPFitmentResponse> ReceiveFitmentAsync(HSRPFitmentRequestData request)
        {
            return await _hsrpRepo.ReceiveFitmentAsync(request);
        }
    }
}
