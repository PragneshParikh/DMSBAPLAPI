using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.CounterBillRepo
{
    public interface ICounterBillRepo
    {
        Task<int> SaveCounterBillAsync(CounterBillViewModel model, string userName);
        Task<int> UpdateCounterBillAsync(CounterBillViewModel model, string userName, int id);
        Task<CounterBillViewModel> GetCounterBillById(int id);
        Task<List<CounterBillViewModel>> GetAllCounterBills(string? dealerCode, DateTime? fromDate, DateTime? toDate, string? search,string? dealerFilter);
        Task<bool> DeleteCounterBill(int counterBillId, string userName);
        Task<List<CounterBillExcelViewModel>> GetAllCounterBillForExcel(string? dealerCode);
    }
}
