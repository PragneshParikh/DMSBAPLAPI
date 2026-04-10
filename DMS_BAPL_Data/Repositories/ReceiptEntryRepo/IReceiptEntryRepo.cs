using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ReceiptEntryRepo
{
    public interface IReceiptEntryRepo
    {
        Task<string?> GetLastReceiptNoAsync();
        Task<ReceiptEntry> AddReceiptEntryAsync(ReceiptEntryViewModel receiptEntry, string userId);
        // Task<List<ReceiptEntry>> GetReceiptEntryListAsync();
        //Task<List<ReceiptEntry>> GetReceiptEntryListAsync(ReceiptFilterViewModel filter);
        Task<List<ReceiptEntryEditViewModel>> GetReceiptEntryListAsync(ReceiptFilterViewModel filter);
            Task<List<LedgerMaster>> GetLedgerMasterDetailsByTypeAsync(string ledgerType);
       Task<ReceiptEntryEditViewModel?> GetReceiptByIdAsync(int id);
        Task<ReceiptEntry?> UpdateReceiptEntryAsync(int id, ReceiptEntryViewModel receiptEntry, string userId);
        Task<bool> CheckReceiptExist(string? mobileNo, string? bookingId);
        Task<List<ReceiptEntryEditViewModel>> GetReceiptEntryListAsyncWithSearch(string? searchTerm,DateOnly? fromDate,DateOnly? toDate);



    }
}
