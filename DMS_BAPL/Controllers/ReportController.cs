using DMS_BAPL_Data.Services.JobCardService;
using DMS_BAPL_Data.Services.StockReportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/Report")]
    public class ReportController : ControllerBase
    {
        private readonly IStockReportService _stockReportService;
        private readonly IJobReportService _jobReportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IStockReportService stockReportService,
            IJobReportService jobReportService,
            ILogger<ReportController> logger)
        {
            _stockReportService = stockReportService;
            _jobReportService = jobReportService;
            _logger = logger;
        }

        /// <summary>
        /// Get Dealer Wise Stock Report
        /// </summary>
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

                var result = await _stockReportService.GetDealerWiseReportAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer wise stock report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get Job Card Report with pagination and filtering
        /// </summary>
        [HttpPost("job-card")]
        [ProducesResponseType(typeof(JobReportPagedResponse<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardReport([FromBody] JobReportFilterModel filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                var result = await _jobReportService.GetJobReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get Dealer Wise Job Card Summary Report
        /// </summary>
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

                var result = await _jobReportService.GetDealerWiseJobReportAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer wise job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get Job Report for specific dealer
        /// </summary>
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

                var result = await _jobReportService.GetJobReportByDealerAsync(dealerCode, pageIndex, pageSize, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching job card report for dealer: {dealerCode}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get filtered Job Card Report
        /// </summary>
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

                var result = await _jobReportService.GetFilteredJobReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Export Job Card Report
        /// </summary>
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

                var result = await _jobReportService.GetJobReportForExportAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get Job Report Summary Statistics
        /// </summary>
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

                var result = await _jobReportService.GetReportSummaryStatsAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job report summary stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}