using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EbwInvoiceService
{
    public interface IEbwInvoiceService
    {
        Task<int> SaveAsync(EbwInvoiceSaveViewModel model, string userId);
        //Task<EbwInvoiceHeader?> GetByIdAsync(int id);
        Task<object?> GetByIdAsync(int id);
        Task<List<object>> GetAllAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<bool> DeleteAsync(int id);
        Task<object?> GetDealerInfoAsync(string dealerCode);
        Task<List<object>> GetReportDataAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<(string PrefixNo, int NextNo)> GetNextPrefixNoAsync(string dealerCode);
    }
}
