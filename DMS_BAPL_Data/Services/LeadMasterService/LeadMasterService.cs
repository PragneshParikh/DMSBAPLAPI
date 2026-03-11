using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LeadMasterRepo;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LeadMasterService
{
    public class LeadMasterService : ILeadMasterService
    {
        private readonly ILeadMasterRepo _leadMasterRepo;
        public LeadMasterService(ILeadMasterRepo leadMasterRepo)
        {
            _leadMasterRepo = leadMasterRepo;
        }
        async Task<LeadViewModel> ILeadMasterService.InsertLmsleadMasterAsync(LeadViewModel leadViewModel)
        {
            try
            {
                return await _leadMasterRepo.InsertLmsleadAsync(leadViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<LmsleadMaster>> GetAlllmsleadMasters()
        {
            return await _leadMasterRepo.GetAlllmsleadMasters();


        }

       
    }
}
