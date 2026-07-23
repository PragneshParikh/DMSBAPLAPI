using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.ReportRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ReportService
{
    public class ReportService : IReportService
    {
        private readonly IReportRepo _reportRepo;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IReportRepo reportRepo,
            ILogger<ReportService> logger)
        {
            _reportRepo = reportRepo;
            _logger = logger;
        }

        // =================================================================
        // STOCK REPORT
        // =================================================================

        public async Task<List<StockReportViewModel>> GetDealerWiseStockReportAsync(string? dealerCode)
        {
            return await _reportRepo.GetDealerWiseStockReportAsync(dealerCode);
        }

        // =================================================================
        // JOB REPORT
        // =================================================================

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(
            JobReportFilterModel filter)
        {
            try
            {
                _logger.LogInformation("Fetching job report");
                return await _reportRepo.GetJobReportAsync(filter);
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
                return await _reportRepo.GetDealerWiseJobReportAsync(dealerCode, fromDate, toDate);
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
                return await _reportRepo.GetJobReportByDealerAsync(
                    dealerCode, pageIndex, pageSize, fromDate, toDate);
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
                return await _reportRepo.GetFilteredJobReportAsync(filter);
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
                return await _reportRepo.GetJobReportForExportAsync(dealerCode, fromDate, toDate);
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
                    PageSize = 100000
                };

                var reportData = await _reportRepo.GetJobReportAsync(filter);
                var completedStatuses = new[] { "Closed", "Complete", "FFIR Closed" };
                var completedJobs = reportData.Data.Count(x => completedStatuses.Contains(x.JobStatus));
                var pendingJobs = reportData.TotalRecords - completedJobs;

                return new JobReportSummaryStats
                {
                    TotalJobs = reportData.TotalRecords,
                    CompletedJobs = completedJobs,
                    PendingJobs = pendingJobs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching summary stats");
                throw;
            }
        }

        // =================================================================
        // VEHICLE SALE REPORT
        // =================================================================

        public async Task<List<VehicleSaleReportViewModel>> GetVehicleSaleReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            return await _reportRepo.GetVehicleSaleReportAsync(fromDate, toDate, dealerCode);
        }

        // =================================================================
        // TOTAL SALE REPORT (DEALER-WISE MAPPING)
        // =================================================================
        public async Task<TotalSaleReportDealerWiseResponse> GetTotalSaleReportDealerWiseAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            try
            {
                return await _reportRepo.GetTotalSaleReportDealerWiseAsync(fromDate, toDate, dealerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching total sale report (dealer wise)");
                throw;
            }
        }

        // =================================================================
        // MODEL WISE SALE REPORT (COUNT-WISE)
        // =================================================================
        public async Task<ModelWiseSalePivotResponse> GetModelWiseSaleCountReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            try
            {
                return await _reportRepo.GetModelWiseSaleCountReportAsync(fromDate, toDate, dealerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model wise sale count report");
                throw;
            }
        }

        // =================================================================
        // MODEL-WISE CURRENT STOCK (COUNT-WISE)
        // =================================================================
        public async Task<ModelWiseStockPivotResponse> GetModelWiseStockCountReportAsync(
            string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                return await _reportRepo.GetModelWiseStockCountReportAsync(dealerCode, fromDate, toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model-wise current stock count report");
                throw;
            }
        }

        public async Task<PagedResponse<CurrentStockReportViewModel>> GetCurrentStockReportAsync(
            CurrentStockFilterModel filter)
        {
            try
            {
                _logger.LogInformation("Fetching current stock report");
                return await _reportRepo.GetCurrentStockReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current stock report");
                throw;
            }
        }

        public async Task<PagedResponse<POTrackingReportViewModel>> GetPOTrackingReportAsync(
            POTrackingFilterModel filter)
        {
            try
            {
                _logger.LogInformation("Fetching PO Tracking Report");
                return await _reportRepo.GetPOTrackingReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PO Tracking Report");
                throw;
            }
        }

        public async Task<List<string>> GetPOTypeDropdownAsync()
        {
            try
            {
                return await _reportRepo.GetPOTypeDropdownAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PO Type dropdown");
                throw;
            }
        }

        public async Task<List<string>> GetPOStatusDropdownAsync()
        {
            try
            {
                return await _reportRepo.GetPOStatusDropdownAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PO Status dropdown");
                throw;
            }
        }

        public async Task<List<PartsDispatchReportViewModel>> GetPartsDispatchReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            try
            {
                return await _reportRepo.GetPartsDispatchReportAsync(fromDate, toDate, dealerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Parts Dispatch Report");
                throw;
            }
        }

        public async Task<List<object>> GetDealerListAsync()
        {
            return await _reportRepo.GetDealerListAsync();
        }

        public async Task<List<object>> GetModelListAsync()
        {
            return await _reportRepo.GetModelListAsync();
        }

        public async Task<List<object>> GetModelListByDealerAsync(string dealerCode)
        {
            return await _reportRepo.GetModelListByDealerAsync(dealerCode);
        }

        public async Task<List<string>> GetChassisListAsync()
        {
            return await _reportRepo.GetChassisListAsync();
        }

        public async Task<List<PartDispatchKitReportViewModel>> GetPartDispatchKitReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            return await _reportRepo.GetPartDispatchKitReportAsync(fromDate, toDate, dealerCode);
        }

        public async Task<List<string>> GetPartDispatchKitPOTypeDropdownAsync()
        {
            return await _reportRepo.GetPartDispatchKitPOTypeDropdownAsync();
        }

        // FORM 22 REPORT FOR SALEBILL
        public async Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo)
        {
            try
            {
                return await _reportRepo.GenerateForm22Report(chassisNo);
            }
            catch
            {
                throw;
            }
        }

        // =================================================================
        // VEHICLE SALE BILL REPORT
        // =================================================================

        public async Task<VehicleSaleBillReportResponse> GetVehicleSaleBillReportAsync(VehicleSaleBillReportFilterModel filter)
        {
            try
            {
                filter ??= new VehicleSaleBillReportFilterModel();
                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                return await _reportRepo.GetVehicleSaleBillReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle sale bill report");
                throw;
            }
        }

        public async Task<List<VehicleSaleBillReportViewModel>> GetVehicleSaleBillReportForExportAsync(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                return await _reportRepo.GetVehicleSaleBillReportForExportAsync(
                    dealerCode, fromDate, toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting vehicle sale bill report");
                throw;
            }
        }

        public async Task<List<string>> GetSaleTypeDropdownAsync()
        {
            try
            {
                return await _reportRepo.GetSaleTypeDropdownAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sale type dropdown");
                throw;
            }
        }

        public async Task<List<string>> GetSaleBillStatusDropdownAsync()
        {
            try
            {
                return await _reportRepo.GetSaleBillStatusDropdownAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sale bill status dropdown");
                throw;
            }
        }

        public async Task<CounterBillPrintViewModel?> GetCounterBillPrintById(int id)
        {
            try
            {
                return await _reportRepo.GetCounterBillPrintById(id);
            }
            catch
            {
                throw;
            }
        }

        // ── NEW: was on ReportRepo/IReportRepo already, but never wrapped here —
        // added now so ReportService fully implements IReportService.
        public async Task<VehicleSaleBillReportResponse> GetVehicleSaleBillOnlyReportAsync(VehicleSaleBillReportFilterModel filter)
        {
            try
            {
                filter ??= new VehicleSaleBillReportFilterModel();
                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                return await _reportRepo.GetVehicleSaleBillOnlyReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle sale bill only report");
                throw;
            }
        }

        // =================================================================
        // VEHICLE INWARD REPORT
        // =================================================================

        public async Task<VehicleInwardReportResponse> GetVehicleInwardReportAsync(VehicleInwardReportFilterModel filter)
        {
            try
            {
                filter ??= new VehicleInwardReportFilterModel();
                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                return await _reportRepo.GetVehicleInwardReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle inward report");
                throw;
            }
        }

        // =================================================================
        // MODEL-WISE VARIANT STOCK (COUNT-WISE)
        // =================================================================
        public async Task<ModelWiseVariantStockPivotResponse> GetModelWiseVariantStockCountReportAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                return await _reportRepo.GetModelWiseVariantStockCountReportAsync(dealerCode, fromDate, toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model-wise variant stock count report");
                throw;
            }
        }
        public async Task<D2DReportResponse> GetD2DReportAsync(D2DReportFilterModel filter)
            => await _reportRepo.GetD2DReportAsync(filter);

        public async Task<List<D2DReportViewModel>> GetD2DReportForExportAsync(D2DReportFilterModel filter)
            => await _reportRepo.GetD2DReportForExportAsync(filter);

        // =================================================================
        // MATERIAL TRANSFER REPORT
        // =================================================================
        public async Task<MaterialTransferReportPagedResponse> GetMaterialTransferReportAsync(MaterialTransferReportFilterModel filter)
        {
            try
            {
                return await _reportRepo.GetMaterialTransferReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching material transfer report");
                throw;
            }
        }

        public async Task<List<MaterialTransferReportRowViewModel>> GetMaterialTransferReportForExportAsync(MaterialTransferReportFilterModel filter)
        {
            try
            {
                return await _reportRepo.GetMaterialTransferReportForExportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting material transfer report");
                throw;
            }
        }

        // =================================================================
        // REPAIR BILL REPORT
        // =================================================================
        public async Task<RepairBillReportPagedResponse> GetRepairBillReportAsync(RepairBillReportFilterModel filter)
        {
            try
            {
                return await _reportRepo.GetRepairBillReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching repair bill report");
                throw;
            }
        }

        public async Task<List<RepairBillReportRowViewModel>> GetRepairBillReportForExportAsync(RepairBillReportFilterModel filter)
        {
            try
            {
                return await _reportRepo.GetRepairBillReportForExportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting repair bill report");
                throw;
            }
        }

        // =================================================================
        // COMPARISON REPORT
        // =================================================================
        public async Task<ComparisonReportPagedResponse> GetComparisonReportAsync(ComparisonReportFilterModel filter)
        {
            try
            {
                return await _reportRepo.GetComparisonReportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comparison report");
                throw;
            }
        }

        public async Task<List<ComparisonReportRowViewModel>> GetComparisonReportForExportAsync(ComparisonReportFilterModel filter)
        {
            try
            {
                return await _reportRepo.GetComparisonReportForExportAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting comparison report");
                throw;
            }
        }

        Task<IEnumerable<Object>> IReportService.GetPartsStockDetailsByDealer(int groupId, string? dealerCode) => _reportRepo.GetPartsStockDetailsByDealer(groupId, dealerCode);
    }
}