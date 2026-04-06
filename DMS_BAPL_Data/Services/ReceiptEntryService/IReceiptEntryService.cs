using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ReceiptEntryService
{
    public interface IReceiptEntryService
    {
        Task<string> GenerateNextReceiptNoAsync();
        Task<ReceiptEntry> AddReceiptEntryAsync(ReceiptEntryViewModel receiptEntry, string userId);
        // Task<List<ReceiptEntry>> GetReceiptEntryListAsync();
        Task<List<ReceiptEntry>> GetReceiptEntryListAsync(ReceiptFilterViewModel filter);
        Task<List<LedgerMaster>> GetLedgerMasterDetailsByTypeAsync(string ledgerType);
        Task<ReceiptEntry?> UpdateReceiptEntryAsync(int id, ReceiptEntryViewModel receiptEntry, string userId);
        Task<ReceiptEntryEditViewModel?> GetReceiptByIdAsync(int id);
        Task<bool> CheckReceiptExist(string? mobileNo, string? bookingId);
        Task<byte[]> downloadReceiptExcel();
        Task<List<ReceiptEntryEditViewModel>> GetReceiptEntryListAsyncWithSearch(string? searchTerm, DateOnly? fromDate, DateOnly? toDate);

    }
}
