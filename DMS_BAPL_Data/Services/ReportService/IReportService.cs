using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ReportService
{
    public interface IReportService
    {
        // ─── Stock ────────────────────────────────────────────────────────────
        Task<List<StockReportViewModel>>GetDealerWiseStockReportAsync(string? dealerCode);
        //Task<List<StockReportViewModel>> GetColourWiseStockReportAsync();
        //Task<List<StockReportViewModel>> GetDealerWiseStockReportAsync(string? dealerCode = null);

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

        Task<JobReportSummaryStats> GetReportSummaryStatsAsync(
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

        Task<List<PartsDispatchReportViewModel>> GetPartsDispatchReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode);

        Task<List<object>> GetDealerListAsync();



        Task<List<PartDispatchKitReportViewModel>> GetPartDispatchKitReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode);

        Task<List<string>> GetPartDispatchKitPOTypeDropdownAsync();
        Task<List<object>> GetModelListAsync();

        Task<List<string>> GetChassisListAsync();

        Task<List<object>> GetModelListByDealerAsync(string dealerCode);
        Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo);

        // ── Vehicle Sale Bill Report ──────────────────────────────────────

        //Task<VehicleSaleBillReportPagedResponse> GetVehicleSaleBillReportAsync(
        //    VehicleSaleBillReportFilterModel filter);
        Task<List<VehicleSaleBillReportViewModel>> GetVehicleSaleBillReportForExportAsync(
            string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<string>> GetSaleTypeDropdownAsync();
        Task<List<string>> GetSaleBillStatusDropdownAsync();

        Task<VehicleSaleBillReportResponse> GetVehicleSaleBillReportAsync(VehicleSaleBillReportFilterModel filter);
        Task<CounterBillPrintViewModel?> GetCounterBillPrintById(int id);

        Task<VehicleSaleBillReportResponse> GetVehicleSaleBillOnlyReportAsync(VehicleSaleBillReportFilterModel filter);


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