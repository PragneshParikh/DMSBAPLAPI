using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DMS_BAPL_Data.Services.JobCardService
{
    public interface IJobReportService
    {
        /// <summary>
        /// Get Job Card Report with pagination and filtering
        /// </summary>
        Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(JobReportFilterModel filter);
        /// <summary>
        /// Get Dealer Wise Job Card Summary Report
        /// </summary>
        Task<List<DealerWiseJobReportSummary>> GetDealerWiseJobReportAsync(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate);
        /// <summary>
        /// Get Job Report for specific dealer
        /// </summary>
        Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportByDealerAsync(
            string dealerCode,
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate);
        /// <summary>
        /// Get Job Report with advanced filtering
        /// </summary>
        Task<JobReportPagedResponse<JobReportViewModel>> GetFilteredJobReportAsync(JobReportFilterModel filter);
        /// <summary>
        /// Export Job Report data
        /// </summary>
        Task<List<JobReportViewModel>> GetJobReportForExportAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate);
        /// <summary>
        /// Get summary statistics
        /// </summary>
        Task<JobReportSummaryStats> GetReportSummaryStatsAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate);
    }
    public class JobReportSummaryStats
    {
        public int TotalJobs { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalTaxes { get; set; }
        public int CompletedJobs { get; set; }
        public int PendingJobs { get; set; }
        public decimal AverageJobValue { get; set; }
    }
}