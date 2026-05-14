using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.ReportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/Report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IReportService reportService,
            ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // STOCK
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get Dealer Wise Stock Report</summary>
        [HttpGet("dealer-wise")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDealerWiseReport()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetDealerWiseStockReportAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer wise stock report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // JOB CARD
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get Job Card Report with pagination and filtering</summary>
        [HttpPost("job-card")]
        [ProducesResponseType(typeof(JobReportPagedResponse<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardReport([FromBody] JobReportFilterModel filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                _logger.LogInformation("Job Card Report API Called");

                var result = await _reportService.GetJobReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>Get Dealer Wise Job Card Summary Report</summary>
        [HttpGet("job-card/dealer-wise")]
        [ProducesResponseType(typeof(List<DealerWiseJobReportSummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardDealerWiseReport(
            [FromQuery] string? dealerCode,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetDealerWiseJobReportAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer wise job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Job Report for specific dealer</summary>
        [HttpGet("job-card/{dealerCode}")]
        [ProducesResponseType(typeof(JobReportPagedResponse<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardReportByDealer(
            string dealerCode,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetJobReportByDealerAsync(
                    dealerCode, pageIndex, pageSize, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching job card report for dealer: {dealerCode}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get filtered Job Card Report</summary>
        [HttpPost("job-card/filter")]
        [ProducesResponseType(typeof(JobReportPagedResponse<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilteredJobCardReport([FromBody] JobReportFilterModel filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetFilteredJobReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Export Job Card Report</summary>
        [HttpGet("job-card/export")]
        [ProducesResponseType(typeof(List<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportJobCardReport(
            [FromQuery] string dealerCode,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetJobReportForExportAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Job Report Summary Statistics</summary>
        [HttpGet("job-card/summary-stats")]
        [ProducesResponseType(typeof(JobReportSummaryStats), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobReportSummaryStats(
            [FromQuery] string dealerCode,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetReportSummaryStatsAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job report summary stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // VEHICLE SALE
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get Vehicle Sale Report</summary>
        [HttpGet("vehicle-sale")]
        [ProducesResponseType(typeof(List<VehicleSaleReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVehicleSaleReport(
            [FromQuery] string? dealerCode,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var result = await _reportService.GetVehicleSaleReportAsync(fromDate, toDate, dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle sale report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("current-stock")]
        [ProducesResponseType(typeof(PagedResponse<CurrentStockReportViewModel>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentStockReport([FromBody] CurrentStockFilterModel filter)
        {
            try
            {
                if (filter == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Filter model is null"
                    });
                }

                if (filter.PageIndex < 1)
                    filter.PageIndex = 1;

                if (filter.PageSize < 1)
                    filter.PageSize = 20;

                var result = await _reportService
                    .GetCurrentStockReportAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current stock report");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}