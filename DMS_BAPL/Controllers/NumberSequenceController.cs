using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace DMS_BAPL_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/number-sequence")]
    public class NumberSequenceController : ControllerBase
    {
        private readonly IPrefixService _prefixService;
        private readonly ILogger<NumberSequenceController> _logger;

        public NumberSequenceController(
            IPrefixService prefixService,
            ILogger<NumberSequenceController> logger)
        {
            _prefixService = prefixService;
            _logger = logger;
        }

        [HttpGet("modules")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModules()
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User not authorized");

            try
            {
                var sequences = await _prefixService.Get();
                var moduleNames = sequences
                    .Select(sequence => sequence.SequenceName)
                    .Append(NumberSequenceModules.StockAdjustment)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return Ok(moduleNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving number sequence modules");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to retrieve number sequence modules.");
            }
        }

        [HttpGet("stock-adjustment")]
        [ProducesResponseType(typeof(StockAdjustmentNumberSequenceViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStockAdjustmentSequence([FromQuery] string? dealerCode)
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User not authorized");

            if (GetUserInfoFromToken.GetUserGroup(HttpContext))
            {
                if (string.IsNullOrWhiteSpace(dealerCode))
                    return BadRequest("Dealer code is required.");
            }
            else
            {
                dealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                if (string.IsNullOrWhiteSpace(dealerCode))
                    return Unauthorized("Dealer code not found in the authenticated user's token.");
            }
            dealerCode = dealerCode.Trim();

            try
            {
                var sequence = await _prefixService.GetActiveSequenceByDealerAndModule(
                    dealerCode,
                    NumberSequenceModules.StockAdjustment);

                if (sequence == null)
                    return BadRequest("Stock Adjustment Number Sequence Configuration is not configured for the selected dealer.");

                if (string.IsNullOrWhiteSpace(sequence.SequenceCode) ||
                    sequence.NextNo < 0 ||
                    sequence.Increment <= 0)
                {
                    return BadRequest("Stock Adjustment Number Sequence Configuration is incomplete.");
                }

                string generatedNumber;
                try
                {
                    generatedNumber = NumberSequenceNumberFormatter.Format(
                        sequence.SequenceCode,
                        sequence.NextNo);
                }
                catch (ArgumentException)
                {
                    return BadRequest("Stock Adjustment Number Sequence Configuration has an invalid prefix format.");
                }

                return Ok(new StockAdjustmentNumberSequenceViewModel
                {
                    DealerCode = dealerCode,
                    Prefix = sequence.SequenceCode,
                    NextNumber = sequence.NextNo,
                    GeneratedNumber = generatedNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Stock Adjustment number sequence for dealer {DealerCode}", dealerCode);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to retrieve Stock Adjustment number sequence.");
            }
        }
    }
}
