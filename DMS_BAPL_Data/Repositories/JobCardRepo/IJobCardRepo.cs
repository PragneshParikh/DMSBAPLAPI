using DMS_BAPL_Data.CustomModel;
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
        Task<List<JobCardDetailsViewModel>> GetJobCardListViewAsync(string dealerCode);
        Task<int> InsertJobCardinfoDetails(JobCardDetailsViewModel jobCardDetails);
        Task<int> UpdateJobCardinfoDetails(UpdateJobCardVM updateJobCardDetails);
        Task<PagedResponse<object>> GetFilterdJobCardDetails(DateTime? fromDate, DateTime? toDate, int? jobNo, int? manualJobNo, int pageIndex, int pageSize);
        Task<int> UpdateSaleDetails(UpdateSaleDetailsVM updateSale);
        Task<int> DeleteJobCard(int jobId);
        Task<List<JobCardDetailsViewModel>> SearchJobCards(JobCardSearchModel model);

        Task<JobCardHeader> GetJobCardById(int Id);
    }
}
