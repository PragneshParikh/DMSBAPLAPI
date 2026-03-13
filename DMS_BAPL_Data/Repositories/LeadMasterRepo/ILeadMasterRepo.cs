using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LeadMasterRepo
{
    public interface ILeadMasterRepo
    {
        Task<LeadViewModel> InsertLmsleadAsync(LeadViewModel leadViewModel);
        Task<List<LmsleadMaster>> GetAlllmsleadMasters();

    }
}
