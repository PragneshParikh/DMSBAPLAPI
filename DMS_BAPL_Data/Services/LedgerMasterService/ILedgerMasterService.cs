using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
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
        Task<PagedResponse<LedgerMaster>> GetLedgerByPagedAsync(string? searchTerm, int pageIndex, int pageSize);
        Task<LedgerMaster?> GetLedgerByIdAsync(int id);
        Task<int> InsertLedgerDetail(LedgerMaster ledgerMaster, string userId);
        Task<bool> UpdateLedgerDetail(LedgerMaster ledgerMaster, string userId);
    }
}
