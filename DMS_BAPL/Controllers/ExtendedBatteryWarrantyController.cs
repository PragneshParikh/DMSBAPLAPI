using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.ExtendedBatteryWarrantyService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/extended-battery-warranty")]
    [ApiController]
    public class ExtendedBatteryWarrantyController : ControllerBase
    {
        private readonly IExtendedBatteryWarrantyService _extendedBatteryWarrantyService;
        private readonly ILogger<ExtendedBatteryWarrantyController> _logger;

        public ExtendedBatteryWarrantyController(IExtendedBatteryWarrantyService extendedBatteryWarrantyService, ILogger<ExtendedBatteryWarrantyController> logger)
        {
            _extendedBatteryWarrantyService = extendedBatteryWarrantyService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ExtendedBatteryWarranty>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ExtendedBatteryWarranty>>> GetAll()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var extendedBatteryWarranty = await _extendedBatteryWarrantyService.Get();

                return Ok(extendedBatteryWarranty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching extended battery warranty data: ${ex.Message}");
                throw;
            }
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<object>>> GetExtendedBatteryWarrantyByPaged(string? searchTerm, int pageIndex, int pageSize)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var extendWarranty = await _extendedBatteryWarrantyService.GetExtendedBatteryWarrantyByPaged(searchTerm, pageIndex, pageSize);

                return Ok(extendWarranty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featcing extended battry warranty data by paged : ${ex.Message}");
                throw;
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ExtendedBatteryWarranty), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExtendedBatteryWarranty>> GetSchemeDetailById(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var extendedBattery = await _extendedBatteryWarrantyService.GetSchemeDetailById(id);

                return Ok(extendedBattery);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featcing data by Id : ${ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Insert([FromBody] ExtendedBatteryWarrantyViewModel extendedBatteryWarrantyViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var extendedWarranty = _extendedBatteryWarrantyService.Insert(extendedBatteryWarrantyViewModel);

                return Ok(extendedWarranty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while insert data into extended warranty : ${ex.Message}");
                throw;
            }
        }

        [HttpPut("{Id}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] ExtendedBatteryWarrantyViewModel extendedBatteryWarrantyViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var extendedWarranty = await _extendedBatteryWarrantyService.Update(extendedBatteryWarrantyViewModel);

                return Ok(extendedWarranty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating the record : ${ex.Message}");
                throw;
            }
        }
    }
}
