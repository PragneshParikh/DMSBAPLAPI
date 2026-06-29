using DMS_BAPL_Data.Repositories.CounterBillRepo;
using DMS_BAPL_Data.Repositories.PrefixRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.CounterBillService
{
    public class CounterBillService : ICounterBillService
    {
        private readonly ICounterBillRepo _repo;
        private readonly IPrefixRepo _prefixRepo;
        private readonly IExcelService _excelService;
        public CounterBillService(ICounterBillRepo repo, IPrefixRepo prefixRepo, IExcelService excelService)
        {
            _repo = repo;
            _prefixRepo = prefixRepo;
            _excelService = excelService;
        }

        public async Task<int> SaveCounterBillAsync(CounterBillViewModel model, string userName)
        {
            var result = await _repo.SaveCounterBillAsync(model, userName);
            if (result != 0)
            {

                await _prefixRepo.UpdateNextNumberByDealerByModule(model.Header.DealerCode, "counter-bill");

            }
            return result;
        }

        public async Task<int> UpdateCounterBillAsync(CounterBillViewModel model, string userName, int id)
        {
            try
            {
                return await _repo.UpdateCounterBillAsync(model, userName, id);
            }
            catch { throw; }
        }

        public async Task<CounterBillViewModel> GetCounterBillById(int id)
        {
            try
            {
                return await _repo.GetCounterBillById(id);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<CounterBillViewModel>> GetAllCounterBills(string? dealerCode, DateTime? fromDate, DateTime? toDate, string? search, string? dealerFilter)
        {
            try
            {
                return await _repo.GetAllCounterBills(dealerCode,fromDate,toDate,search,dealerFilter);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteCounterBill(int counterBillId, string userName)
        {
            try
            {
                return await _repo.DeleteCounterBill(counterBillId, userName);
            }
            catch
            {
                throw;
            }
        }
        public async Task<byte[]> DownloadCounterBillExcel(string? dealerCode, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                var data = await _repo.GetAllCounterBillForExcel(dealerCode);
                if (dateFrom.HasValue)
                {
                    data = data.Where(x => x.BillDate.Date >= dateFrom.Value.Date).ToList();
                }

                if (dateTo.HasValue)
                {
                    data = data.Where(x => x.BillDate.Date <= dateTo.Value.Date).ToList();
                }

                // DTO Properties
                var properties = typeof(CounterBillExcelViewModel).GetProperties().ToList();

                // Excel Columns
                var columns = properties.Select(p => p.Name).ToList();

                // Excel Rows
                var rows = data.Select(item =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        dict[prop.Name] = prop.GetValue(item) ?? "";
                    }

                    return dict;
                }).ToList();

                // Excel Model
                var model = new ExcelExportViewModel
                {
                    SheetName = "Counter Bill Report",
                    Columns = columns,
                    Rows = rows
                };

                // Generate Excel
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
