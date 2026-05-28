using DMS_BAPL_Data.Services.ChassisService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
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
        [HttpPost("import")]
        public async Task<IActionResult> ImportChassisExcel([FromForm] ChassisImportRequest request)
        {
            try
            {
                if (request.File == null)
                    return BadRequest(
                        "File not found");

                var result =
                    await _chassisService
                        .ImportChassisExcelAsync(
                            request.File);

                return Ok(new
                {
                    message = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error importing chassis excel");

                return StatusCode(
                    500,
                    ex.Message);
            }
        }
    }
}
