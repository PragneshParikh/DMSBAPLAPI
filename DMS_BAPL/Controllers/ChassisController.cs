using DMS_BAPL_Data.Services.ChassisService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/chassis")]
    [ApiController]
    public class ChassisController : ControllerBase
    {
        private readonly IChassisService _chassisService;
        private readonly ILogger<ChassisController> _logger;

        public ChassisController(IChassisService chassisService, ILogger<ChassisController> logger)
        {
            _chassisService = chassisService;
            _logger = logger;
        }

        [HttpGet("{chassisNumber}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChassisDetailByChassis(string chassisNumber)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var chassisData = await _chassisService.GetChassisDataAsync(chassisNumber);

                return Ok(chassisData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while getting chassis data : ${ex.Message}");
                throw;
            }
        }
    }
}
