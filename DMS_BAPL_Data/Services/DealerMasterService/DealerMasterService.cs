using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DealerMasterService
{
    public class DealerMasterService:IDealerMasterService
    {
        private readonly IDealerMasterRepo _dealerMasterRepo;

        public DealerMasterService(IDealerMasterRepo dealerMasterRepo)
        {
            _dealerMasterRepo = dealerMasterRepo;
        }
       

        public async Task<DealerMaster> AddDealerAsync(DealerMasterDto dealer)
        {
            return await _dealerMasterRepo.AddDealerAsync(dealer);
        }

        public async Task<List<DealerMaster>> GetAllDealersAsync()
        {
            return await _dealerMasterRepo.GetAllDealersAsync();
        }
        
        public async Task<DealerMaster> GetDealerById(int id)
        {
            return await _dealerMasterRepo.GetDealerById(id);
        }

        public async Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterDto dealer)
        {
            return await _dealerMasterRepo.UpdateDealerAsync(id, dealer);
        }
    }
}
