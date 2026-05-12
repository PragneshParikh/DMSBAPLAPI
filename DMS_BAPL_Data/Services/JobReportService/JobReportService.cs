using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Utils.ViewModels;
using DMS_BAPL_Data.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS_BAPL_Data.Repositories.JobReportRepo;

namespace DMS_BAPL_Data.Services.JobReportService
{
    public class JobReportService : IJobReportService
    {
        private readonly IJobReportRepo _jobReportRepo;
        private readonly ILogger<JobReportService> _logger;

        public JobReportService(
            IJobReportRepo jobReportRepo,
            ILogger<JobReportService> logger)
        {
            _jobReportRepo = jobReportRepo;
            _logger = logger;
        }

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(JobReportFilterModel filter)
        {
            try
            {
                _logger.LogInformation("Fetching job report");

                return await _jobReportRepo.GetJobReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job report");
                throw;
            }
        }

        public async Task<List<DealerWiseJobReportSummary>> GetDealerWiseJobReportAsync(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                return await _jobReportRepo.GetDealerWiseJobReportAsync(
                    dealerCode,
                    fromDate,
                    toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer wise report");
                throw;
            }
        }

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportByDealerAsync(
            string dealerCode,
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                return await _jobReportRepo.GetJobReportByDealerAsync(
                    dealerCode,
                    pageIndex,
                    pageSize,
                    fromDate,
                    toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer report");
                throw;
            }
        }

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetFilteredJobReportAsync(
            JobReportFilterModel filter)
        {
            try
            {
                return await _jobReportRepo.GetFilteredJobReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered report");
                throw;
            }
        }

        public async Task<List<JobReportViewModel>> GetJobReportForExportAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                return await _jobReportRepo.GetJobReportForExportAsync(
                    dealerCode,
                    fromDate,
                    toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                throw;
            }
        }

        public async Task<JobReportSummaryStats> GetReportSummaryStatsAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var filter = new JobReportFilterModel
                {
                    DealerCode = dealerCode,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageIndex = 1,
                    PageSize = 10000
                };

                var reportData = await _jobReportRepo.GetJobReportAsync(filter);

                return new JobReportSummaryStats
                {
                    TotalJobs = reportData.TotalRecords,
                    TotalRevenue = reportData.GrandTotal,
                    TotalTaxes = reportData.TotalSGST + reportData.TotalCGST,
                    CompletedJobs = reportData.TotalRecords,
                    PendingJobs = 0,
                    AverageJobValue = reportData.TotalRecords > 0
                        ? reportData.GrandTotal / reportData.TotalRecords
                        : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching summary stats");
                throw;
            }
        }
    }
}