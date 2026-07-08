using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LedgerMasterService
{
    public interface ILedgerMasterService
    {
        Task<IEnumerable<LedgerMaster>> GetAll();
        Task<PagedResponse<object>> GetLedgerByPagedAsync(string? searchTerm, int pageIndex, int pageSize, string dealerCode,string filter);
        Task<LedgerDetailViewModel?> GetLedgerByIdAsync(int id);
        Task<int> InsertLedgerDetail(LedgerMaster ledgerMaster, string userId);
        Task<bool> UpdateLedgerDetail(LedgerMaster ledgerMaster, string userId);
        Task<IEnumerable<LedgerMaster>> GetCompanyLedgersAsync();
        Task<IEnumerable<LedgerMaster>> GetInsuranceLedgersAsync();
        Task<List<LedgerMaster>> GetLedgerByLedgerType(string ledgerType);
        Task<List<string>> GetAllMobileNumberByDealerCode(string dealerCode);
        Task<string> GetNextLedId(string dealerCode);
        Task<byte[]> DownloadExcel(string? dealerCode);
        Task<List<LedgerMaster>> GetLedgerForSale(string? dealerCode, bool isSuperAdmin);
        Task<IEnumerable<LedgerMaster>> GetLotRelatedLedgers(string? dealerCode, bool? IsD2D);
    }
}
