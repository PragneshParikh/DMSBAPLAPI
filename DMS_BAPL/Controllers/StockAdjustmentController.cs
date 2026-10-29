using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.ReportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/stock-adjustment")]
    public class StockAdjustmentController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<StockAdjustmentController> _logger;

        public StockAdjustmentController(
            IReportService reportService,
            ILogger<StockAdjustmentController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("parts")]
        [HttpGet("~/api/stockadjustment/parts")]
        [ProducesResponseType(typeof(List<StockAdjustmentPartViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchParts(
            [FromQuery] string? search,
            [FromQuery] string? locationCode,
            [FromQuery] string? dealerCode)
        {
            if (!TryGetAuthorizedDealerCode(dealerCode, out dealerCode, out var authResult))
                return authResult!;

            try
            {
                return Ok(await _reportService.SearchStockAdjustmentPartsAsync(
                    dealerCode!,
                    search,
                    locationCode));
            }
            catch (StockAdjustmentValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching stock adjustment parts for dealer {DealerCode}", dealerCode);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to search stock adjustment parts.");
            }
        }

        [HttpGet("parts/{partNo}")]
        [ProducesResponseType(typeof(StockAdjustmentPartDetailsViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartDetails(
            string partNo,
            [FromQuery] string locationCode,
            [FromQuery] string? dealerCode)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest(new { message = "Part number is required." });
            if (string.IsNullOrWhiteSpace(locationCode))
                return BadRequest(new { message = "Location code is required." });
            if (!TryGetAuthorizedDealerCode(dealerCode, out dealerCode, out var authResult))
                return authResult!;

            try
            {
                var part = await _reportService.GetStockAdjustmentPartDetailsAsync(
                    dealerCode!,
                    locationCode,
                    partNo);
                return part == null ? NotFound(new { message = "Part was not found." }) : Ok(part);
            }
            catch (StockAdjustmentValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock adjustment details for part {PartNo}", partNo);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to retrieve stock adjustment part details.");
            }
        }

        [HttpGet("locations")]
        [ProducesResponseType(typeof(List<StockAdjustmentLocationViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocations(
            [FromQuery] string? dealerCode,
            [FromQuery] bool includeAllLocations = true)
        {
            if (!TryGetAuthorizedDealerCode(dealerCode, out dealerCode, out var authResult))
                return authResult!;

            try
            {
                return Ok(await _reportService.GetStockAdjustmentLocationsAsync(
                    dealerCode!,
                    includeAllLocations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock adjustment locations for dealer {DealerCode}", dealerCode);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to retrieve stock adjustment locations.");
            }
        }

        private bool TryGetAuthorizedDealerCode(
            string? requestedDealerCode,
            out string? dealerCode,
            out IActionResult? errorResult)
        {
            dealerCode = requestedDealerCode?.Trim();
            errorResult = null;

            if (string.IsNullOrWhiteSpace(GetUserInfoFromToken.GetUserIdFromToken(HttpContext)))
            {
                errorResult = Unauthorized("User not authorized");
                return false;
            }

            if (GetUserInfoFromToken.GetUserGroup(HttpContext))
            {
                if (string.IsNullOrWhiteSpace(dealerCode))
                {
                    errorResult = BadRequest(new { message = "Dealer code is required." });
                    return false;
                }

                return true;
            }

            dealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)?.Trim();
            if (string.IsNullOrWhiteSpace(dealerCode))
            {
                errorResult = Unauthorized("Dealer code not found in the authenticated user's token.");
                return false;
            }

            return true;
        }
    }
}
