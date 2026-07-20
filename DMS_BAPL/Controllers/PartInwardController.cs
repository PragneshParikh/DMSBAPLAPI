using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.PartsInwardService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/parts-inward")]
    [ApiController]
    public class PartInwardController : ControllerBase
    {
        private readonly IPartInwardService _partInwardService;
        private readonly ILogger<PartInwardController> _logger;

        public PartInwardController(IPartInwardService partInwardService, ILogger<PartInwardController> logger)
        {
            _partInwardService = partInwardService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PartsInward>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                string dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

                if (string.IsNullOrEmpty(dealerCode))
                    return Unauthorized("Dealer Code is missing or invalid.");

                var partInwards = await _partInwardService.Get();

                return Ok(partInwards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching part inward records.");
                throw;
            }
        }

        [HttpGet("NotificationsByDealer/{dealerCode}")]
        [ProducesResponseType(typeof(IEnumerable<PartsInward>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDealer(string dealerCode)
        {
            try
            {
                if (string.IsNullOrEmpty(dealerCode))
                    return BadRequest("Dealer Code is missing or invalid.");

                var partInwards = await _partInwardService.GetPartInwardByDealerAsync(dealerCode);

                return Ok(partInwards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching part inward records for dealer {DealerCode}.", dealerCode);
                throw;
            }
        }

        [HttpGet("GetPendingPartInwardDetailByLocation")]
        [ProducesResponseType(typeof(IEnumerable<PartsInward>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingPartInwardDetailByLocation([FromQuery] string locationCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var parts = await _partInwardService.GetPendingPartInwardDetailByLocation(locationCode);

                return Ok(parts);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PartsInward([FromBody] PartsInwardViewModel partsInwardViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var parts = await _partInwardService.PartsInward(partsInwardViewModel);

                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting part inward data : {ex.Message}");
                throw;
            }
        }

        [HttpPut("UpdateByInvoice")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> UpdateByInvoice([FromBody] string invoiceNo)
        {
            try
            {
                string dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

                if (string.IsNullOrEmpty(dealerCode))
                    return Unauthorized("User not authorized");

                var result = await _partInwardService.UpdateByInvoice(invoiceNo, dealerCode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating the parts status : ${ex.Message}");
                throw;
            }
        }
    }
}