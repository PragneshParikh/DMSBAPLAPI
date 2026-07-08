using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
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
        Task<List<LotInspectionChassisVM>> GetAllInspectedLotChassisAsync(string dealerCode, int jobTypeId);
        Task<List<PdichecklistMaster>> GetPdichecklist(int oemModelId);
        Task<List<JobCardlistDetailsViewModel>> GetJobCardListViewAsync(JobCardSearchVM search);
        Task<int> InsertJobCardinfoDetails(JobCardDetailsViewModel jobCardDetails, string userId);
        Task<int> UpdateJobCardinfoDetails(UpdateJobCardVM updateJobCardDetails);
        Task<PagedResponse<object>> GetFilterdJobCardDetails(DateTime? fromDate, DateTime? toDate, int? jobNo, int? manualJobNo, int pageIndex, int pageSize);
        Task<int> UpdateSaleDetails(UpdateSaleDetailsVM updateSale);
        Task<int> DeleteJobCard(int jobId);
        Task<List<JobCardlistDetailsViewModel>> SearchJobCards(JobCardSearchModel model);
        //Task<JobCardHeader> GetJobCardById(int Id);
        Task<JsonResult> GetJobCardById(int Id);
        Task<List<ServiceHistoryViewModel>> GetServiceHistoryViewModellist(string chassisNo, int? jobCardId);
        Task<CIRJobcardViewModel> GetCIRJobCardDetails(int id);
        Task<int> GetNextJobNumber(string dealerCode);
        Task<List<MaterialedJobCardListVM>> GetMaterialedJobCardList(int? jobId);
        Task<bool> UpdateMaterialTransferStatus(int jobId, bool status);
        Task<InspectedChassisListVM> GetInspectedChassisListDropdown(string dealerCode);
        Task<List<JobCardlistDetailsViewModel>> GetJobCardListRepairBill(JobCardSearchVM search);
        Task<PagedResponse<object>> GetJobCardByStatus(DateTime? fromDate, DateTime? toDate, int? jobNo, int? manualJobNo, bool isClosed, int pageIndex, int pageSize, string? dealerCode);

        Task<List<IssueTypebasedJobDetails>> GetIssueTypebasedJobDetail(string? dealerCode, int? jobNo, string? serviceloc, DateTime? fromDate, DateTime? toDate);
        Task<JobCardPrintVM?> GetJobCardForPrint(int jobId);
        Task<bool> GetJobCardStatusById(int Id);
    }
}
