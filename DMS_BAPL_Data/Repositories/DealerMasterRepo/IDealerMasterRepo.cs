using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DealerMasterRepository
{
    public interface IDealerMasterRepo
    {
        Task<DealerMaster> AddDealerAsync(DealerMasterViewModel dealer, string userId);
        Task<List<DealerMaster>> GetAllDealersAsync(string? search);
        Task<DealerMaster> GetDealerById(int id);
        //Task<DealerMaster?> UpdateDealerAsync(int id, DealerMaster dealer);
        Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterViewModel dealerDto,string userId);
        Task<List<DealerDropdownViewModel>> GetDealerDropdown();
        Task<DealerMaster> GetDealerByCode(string dealerCode);
        Task AddDealerToLedgerAsync(DealerMasterViewModel dealer, string userId);

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        Task SaveAsync();


    }
}
