using DMS_BAPL_Data.Services.ChassisDetailsService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/chassis-details")]
    [ApiController]
    public class ChassisDetailsController : ControllerBase
    {
        private readonly IChassisDetailService _chassisDetailService;
        public ChassisDetailsController(IChassisDetailService chassisDetailService)
        {
            _chassisDetailService = chassisDetailService;
        }

        [HttpGet("chassisList")]
        public async Task<IActionResult> GetChassisDetailBylLocationCode(string locationCode)
        {
            try
            {
                var result = await _chassisDetailService.GetChassisList(locationCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("soldChassis")]
        public async Task<IActionResult> GetSoldChassisDetailsList()
        {
            try
            {
                var result = await _chassisDetailService.GetSoldChassisDetailsList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPut("UpdateNewLedgerForChassis")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateNewLedgerForChassis(
            [FromQuery] string ledgerId,
             [FromQuery] string dealerCode,
             [FromQuery] string chassisNo
            )
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _chassisDetailService.UpdateNewLedgerForChassis(Convert.ToInt32(ledgerId), dealerCode, chassisNo, userId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

    }
}
