using DMS_BAPL_Data.Services.RepoBillingService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/repo-billing")]
    [ApiController]
    public class RepoBillingController : ControllerBase
    {
        private readonly IRepoBillingService _repoBillingService;

        public RepoBillingController(IRepoBillingService repoBillingService)
        {
            _repoBillingService = repoBillingService;
        }

        [HttpGet("GetRepoBillingByChassis")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRepoBillingByChassis([FromQuery] string? chassisNo, [FromQuery] string? regNo)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User not authorized");

                var repoBill = await _repoBillingService.GetRepoBillingByChassis(chassisNo, regNo);

                return Ok(repoBill.Value);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
