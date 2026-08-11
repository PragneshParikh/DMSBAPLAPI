using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.EbwInvoiceRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EbwInvoiceService
{
    public class EbwInvoiceService : IEbwInvoiceService
    {
        private readonly IEbwInvoiceRepo _repo;
        public EbwInvoiceService(IEbwInvoiceRepo repo) { _repo = repo; }

        public Task<int> SaveAsync(EbwInvoiceSaveViewModel model, string userId) => _repo.SaveAsync(model, userId);
        public Task<List<object>> GetAllAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
           => _repo.GetAllAsync(dealerCode, fromDate, toDate);
        //public Task<EbwInvoiceHeader?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<object?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
        public Task<object?> GetDealerInfoAsync(string dealerCode) => _repo.GetDealerInfoAsync(dealerCode);
        public Task<List<object>> GetReportDataAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
             => _repo.GetReportDataAsync(dealerCode, fromDate, toDate);
        public Task<(string PrefixNo, int NextNo)> GetNextPrefixNoAsync(string dealerCode) => _repo.GetNextPrefixNoAsync(dealerCode);
    }
}
