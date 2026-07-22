using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LedgerMasterRepo
{
    public interface ILedgerMasterRepo
    {
        Task<IEnumerable<LedgerMaster>> GetAll();
        Task<PagedResponse<object>> GetLedgerByPagedAsync(string? searchTerm, int pageIndex, int pageSize, string dealerCode, string filter);
        Task<LedgerDetailViewModel?> GetLedgerById(int id);
        Task<int> InsertLedgerDetail(LedgerMaster ledgerMaster);
        Task<bool> UpdateLedgerDetail(LedgerMaster ledgerMaster);
        Task<bool> CheckLedgerExist(string? email, string? mobile);
        Task<LedgerMaster> CreateLedgerFromLead(LmsleadMaster lead, string userId);
        Task<LedgerMaster> GetLedgerByEmailOrMobile(string? email, string? mobile);
        Task<IEnumerable<LedgerDetailViewModel>> GetCompanyLedgers();
        Task<bool?> GetD2DProvision(string? dealerCode);
        Task<IEnumerable<LedgerMaster>> GetInsuranceLedgers();
        Task<List<LedgerMaster>> GetLedgerByLedgerType(string ledgerType);
        Task<List<string>> GetAllMobileNumberByDealerCode(string dealerCode);
        Task<string> GetNextLedCode(string dealerCode);
        Task<IEnumerable<LedgerExcelViewModel>> GetExcelData();
        Task<List<LedgerMaster>> GetLedgerForSale(string? dealerCode, bool isSuperAdmin);
        Task<IEnumerable<LedgerMaster>> GetLotRelatedLedgers(string? invoiceNo, bool? IsD2D);
        Task<IEnumerable<LedgerMaster>> GetSupplierLedgers(string? dealerCode);
        Task<IEnumerable<LedgerMaster>> GetLedgerByLedgerTypes(string[] ledgerTypes);
    }
}
