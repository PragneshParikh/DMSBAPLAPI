using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LeadMasterService
{
    public interface ILeadMasterService
    {

        Task<LeadViewModel> InsertLmsleadMasterAsync(LeadViewModel leadViewModels);
        Task<List<LmsleadMaster>> GetAlllmsleadMasters();
        Task<LmsleadMaster> GetLMSLeadMasterByMobileNo(string? mobileNo, int? bookingId);
    }
}
