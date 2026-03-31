using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LedgerMasterRepo;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LedgerMasterService
{
    public partial class LedgerMasterService : ILedgerMasterService
    {
        private readonly ILedgerMasterRepo _ledgerMasterRepo;
        public LedgerMasterService(ILedgerMasterRepo ledgerMasterRepo)
        {
            _ledgerMasterRepo = ledgerMasterRepo;
        }

        Task<IEnumerable<LedgerMaster>> ILedgerMasterService.GetAll() => _ledgerMasterRepo.GetAll();
        Task<PagedResponse<LedgerMaster>> ILedgerMasterService.GetLedgerByPagedAsync(string? searchTerm, int pageIndex, int pageSize) => _ledgerMasterRepo.GetLedgerByPagedAsync(searchTerm, pageIndex, pageSize);
        Task<LedgerMaster?> ILedgerMasterService.GetLedgerByIdAsync(int id) => _ledgerMasterRepo.GetLedgerById(id);
        Task<int> ILedgerMasterService.InsertLedgerDetail(LedgerMaster ledgerMaster, string userId)
        {
            ledgerMaster.CreatedBy = userId;
            ledgerMaster.CreatedDate = DateTime.Now;
            return _ledgerMasterRepo.InsertLedgerDetail(ledgerMaster);
        }
        Task<bool> ILedgerMasterService.UpdateLedgerDetail(LedgerMaster ledgerMaster, string userId)
        {
            ledgerMaster.UpdatedBy = userId;
            ledgerMaster.UpdatedDate = DateTime.Now;
            return _ledgerMasterRepo.UpdateLedgerDetail(ledgerMaster);
        }
    }
}
