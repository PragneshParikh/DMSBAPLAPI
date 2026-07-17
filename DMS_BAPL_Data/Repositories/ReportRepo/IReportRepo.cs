using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ReportRepo
{
    public interface IReportRepo
    {
        // ── STOCK REPORT ──────────────────────────────────────────────
        Task<List<StockReportViewModel>> GetDealerWiseStockReportAsync( string? dealerCode = null,DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<StockReportViewModel>> GetColourWiseStockReportAsync();



        // ── JOB REPORT ────────────────────────────────────────────────
        Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(JobReportFilterModel filter);
        Task<List<DealerWiseJobReportSummary>> GetDealerWiseJobReportAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportByDealerAsync(string dealerCode, int pageIndex, int pageSize, DateTime? fromDate, DateTime? toDate);
        Task<JobReportPagedResponse<JobReportViewModel>> GetFilteredJobReportAsync(JobReportFilterModel filter);
        Task<List<JobReportViewModel>> GetJobReportForExportAsync(string dealerCode, DateTime? fromDate, DateTime? toDate);

        // ── VEHICLE SALE REPORT ───────────────────────────────────────
        Task<List<VehicleSaleReportViewModel>> GetVehicleSaleReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode);

        // ── TOTAL SALE REPORT (DEALER-WISE MAPPING) ────────────────────
        Task<TotalSaleReportDealerWiseResponse> GetTotalSaleReportDealerWiseAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode);

        // ── MODEL WISE SALE REPORT (COUNT-WISE) — pivoted Dealer x Model ──
        Task<ModelWiseSalePivotResponse> GetModelWiseSaleCountReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode);

        // ── CURRENT STOCK REPORT ──────────────────────────────────────
        Task<PagedResponse<CurrentStockReportViewModel>> GetCurrentStockReportAsync(CurrentStockFilterModel filter);

        // ── MODEL-WISE CURRENT STOCK (COUNT-WISE) — pivoted Dealer x Model ──
        Task<ModelWiseStockPivotResponse> GetModelWiseStockCountReportAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);

        // ── PO TRACKING REPORT ────────────────────────────────────────
        Task<PagedResponse<POTrackingReportViewModel>> GetPOTrackingReportAsync(POTrackingFilterModel filter);
        Task<List<string>> GetPOTypeDropdownAsync();
        Task<List<string>> GetPOStatusDropdownAsync();

        // ── PARTS DISPATCH REPORT ─────────────────────────────────────
        Task<List<PartsDispatchReportViewModel>> GetPartsDispatchReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode);

        // ── DROPDOWNS / LOOKUPS ───────────────────────────────────────
        Task<List<object>> GetDealerListAsync();
        Task<List<object>> GetModelListAsync();
        Task<List<object>> GetModelListByDealerAsync(string dealerCode);
        Task<List<string>> GetChassisListAsync();

        // ── PART DISPATCH KIT REPORT ──────────────────────────────────
        Task<List<PartDispatchKitReportViewModel>> GetPartDispatchKitReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode);
        Task<List<string>> GetPartDispatchKitPOTypeDropdownAsync();

        // ── FORM 22 ───────────────────────────────────────────────────
        Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo);

        // ── VEHICLE SALE BILL REPORT ──────────────────────────────────
        Task<VehicleSaleBillReportResponse> GetVehicleSaleBillReportAsync(VehicleSaleBillReportFilterModel filter);
        Task<List<VehicleSaleBillReportViewModel>> GetVehicleSaleBillReportForExportAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<string>> GetSaleTypeDropdownAsync();
        Task<List<string>> GetSaleBillStatusDropdownAsync();
        Task<CounterBillPrintViewModel?> GetCounterBillPrintById(int id);
        Task<VehicleSaleBillReportResponse> GetVehicleSaleBillOnlyReportAsync(VehicleSaleBillReportFilterModel filter);

        // ── VEHICLE INWARD REPORT ── present in the ReportRepo.cs you shared ──
        Task<VehicleInwardReportResponse> GetVehicleInwardReportAsync(VehicleInwardReportFilterModel filter);

        Task<ModelWiseVariantStockPivotResponse> GetModelWiseVariantStockCountReportAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<D2DReportResponse> GetD2DReportAsync(D2DReportFilterModel filter);

        Task<List<D2DReportViewModel>> GetD2DReportForExportAsync(D2DReportFilterModel filter);


    }
}