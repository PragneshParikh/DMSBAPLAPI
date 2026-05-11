using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DMS_BAPL_Data.Repositories.JobReportRepo
{
    public interface IJobReportRepo
    {
        Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(
            JobReportFilterModel filter);
        Task<List<DealerWiseJobReportSummary>> GetDealerWiseJobReportAsync(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate);
        Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportByDealerAsync(
            string dealerCode,
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate);
        Task<JobReportPagedResponse<JobReportViewModel>> GetFilteredJobReportAsync(
            JobReportFilterModel filter);
        Task<List<JobReportViewModel>> GetJobReportForExportAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate);
    }
}