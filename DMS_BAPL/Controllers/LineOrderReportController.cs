using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.LineOrderReportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/line-order-report")]
    [ApiController]
    public class LineOrderReportController : ControllerBase
    {
        private readonly ILineOrderReportService _lineOrderReportService;
        private readonly ILogger<LineOrderReportController> _logger;

        public LineOrderReportController(
            ILineOrderReportService lineOrderReportService,
            ILogger<LineOrderReportController> logger)
        {
            _lineOrderReportService = lineOrderReportService;
            _logger = logger;
        }

       
        private bool TryApplyDealerScope(LineOrderReportFilterModel filter)
        {
            bool isSuperAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);

            if (isSuperAdmin)
                return true;

            var dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

            if (string.IsNullOrWhiteSpace(dealerCode))
                return false;

            filter.DealerCode = dealerCode;
            return true;
        }

        [HttpGet("GetByDealerPaged")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<object>>> GetLineOrderReport(
            [FromQuery] LineOrderReportFilterModel filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (!TryApplyDealerScope(filter))
                    return Unauthorized("Dealer code not found for user");

                var result = await _lineOrderReportService.GetLineOrderReport(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching line order report : {ex.Message}");
                throw;
            }
        }

        [HttpGet("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadExcel([FromQuery] LineOrderReportFilterModel filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (!TryApplyDealerScope(filter))
                    return Unauthorized("Dealer code not found for user");

                var file = await _lineOrderReportService.DownloadLineOrderExcel(filter);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"LineOrder_{filter.LineType}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}