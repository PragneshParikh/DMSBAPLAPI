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

        public async Task<List<StockReportViewModel>>GetDealerWiseStockReportAsync(string? dealerCode)
        {
            return await _reportRepo.GetDealerWiseStockReportAsync(dealerCode);
        }

        //public async Task<List<StockReportViewModel>> GetDealerWiseStockReportAsync(string? dealerCode = null)
        //{
        //    return await _reportRepo.GetDealerWiseStockReportAsync(dealerCode);
        //}

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
                    PageSize = 10000
                };

                var reportData = await _reportRepo.GetJobReportAsync(filter);

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
    }
}