using DMS_BAPL_Data.Services.StockReportService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/Report")]  // ← updated route name
    public class ReportController : ControllerBase
    {
        private readonly IStockReportService _stockReportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IStockReportService stockReportService,
            ILogger<ReportController> logger)
        {
            _stockReportService = stockReportService;
            _logger = logger;
        }

        [HttpGet("dealer-wise")]
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
    }
}