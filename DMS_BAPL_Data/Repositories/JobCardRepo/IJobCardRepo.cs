using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.JobCardRepo
{
    public interface IJobCardRepo
    {
        Task<List<JobCardViewModel>> GetJobtype();
        Task<List<ServiceDataViewModel>> GetServiceDataByJobType(string jobTypeName);
        Task<List<ServiceHeadViewModel>> GetServiceHead(int jobTypeId);
        Task<List<ServiceTypeViewModel>> GetServiceType(int serviceHeadId);
        Task<List<JobSourceViewModel>> GetJobSource();
        Task<List<LotInspectionChassisVM>> GetAllInspectedLotChassisAsync(string dealerCode);
        Task<List<PdichecklistMaster>> GetPdichecklist();
        Task<int> InsertJobCardinfoDetails(JobCardDetailsViewModel jobCardDetails);
    }
}
