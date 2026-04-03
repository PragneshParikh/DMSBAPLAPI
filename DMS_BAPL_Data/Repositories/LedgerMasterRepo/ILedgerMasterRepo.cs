using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
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
        Task<PagedResponse<LedgerMaster>> GetLedgerByPagedAsync(string? searchTerm, int pageIndex, int pageSize);
        Task<LedgerMaster?> GetLedgerById(int id);
        Task<int> InsertLedgerDetail(LedgerMaster ledgerMaster);
        Task<bool> UpdateLedgerDetail(LedgerMaster ledgerMaster);
        Task<bool> CheckLedgerExist(string? email, string? mobile);
        Task <LedgerMaster>CreateLedgerFromLead(LmsleadMaster lead,string userId);
        Task<LedgerMaster> GetLedgerByEmailOrMobile(string? email, string? mobile);

    }
}
