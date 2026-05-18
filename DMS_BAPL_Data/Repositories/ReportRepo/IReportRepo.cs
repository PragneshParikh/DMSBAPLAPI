using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ReportRepo
{
    public interface IReportRepo
    {
        // ─── Stock ────────────────────────────────────────────────────────────
        Task<List<StockReportViewModel>> GetDealerWiseStockReportAsync();
        Task<List<StockReportViewModel>> GetColourWiseStockReportAsync();

        // ─── Job Report ───────────────────────────────────────────────────────
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

        // ─── Vehicle Sale Report ──────────────────────────────────────────────
        Task<List<VehicleSaleReportViewModel>> GetVehicleSaleReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode);

        Task<PagedResponse<CurrentStockReportViewModel>> GetCurrentStockReportAsync(CurrentStockFilterModel filter);

        Task<PagedResponse<POTrackingReportViewModel>> GetPOTrackingReportAsync(POTrackingFilterModel filter);

        Task<List<string>> GetPOTypeDropdownAsync();
        Task<List<string>> GetPOStatusDropdownAsync();
    }
}