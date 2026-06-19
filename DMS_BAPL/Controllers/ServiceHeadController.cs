using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.JobSourceMasterRepo;
using DMS_BAPL_Data.Repositories.ServiceHeadRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceHeadController : ControllerBase
    {
        private readonly IServiceHeadRepo _serviceHeadRepo;
        private readonly ILogger<ServiceHeadController> _logger;

        public ServiceHeadController(IServiceHeadRepo serviceHeadRepo, ILogger<ServiceHeadController> logger)
        {
            _serviceHeadRepo = serviceHeadRepo;
            _logger = logger;
        }

        [HttpGet("GetAllServiceHead")]
        [ProducesResponseType(typeof(List<ServiceHeadMasterViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllServiceHead()
        {
            try
            {
                var result = await _serviceHeadRepo.GetAllServiceHead();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllServiceHead");
                return StatusCode(500, "An error occurred while retrieving ServiceHead.");
            }
        }

        [HttpPost("InsertServiceHead")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertServiceHead(ServiceHeadMasterViewModel serviceHeadMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _serviceHeadRepo.InsertServiceHead(serviceHeadMasterViewModel, userId);
                if (result == -1)
                    return StatusCode(500, "An error occurred while inserting the serviceHead.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Insert serviceHead");
                return StatusCode(500, "An error occurred while inserting the serviceHead.");
            }
        }
        [HttpPut("UpdateServiceHeadName")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateServiceHeadName([FromBody] ServiceHeadMasterViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _serviceHeadRepo.UpdateServiceHeadName(model, userId);
                if (result == -1)
                    return NotFound($"ServiceHead with ID {model.Id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateServiceHeadName");
                return StatusCode(500, "An error occurred while updating the ServiceHead name.");
            }
        }

        [HttpDelete("DeleteServiceHead/{serviceHeadId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteServiceHead(int serviceHeadId)
        {
            try
            {
                var result = await _serviceHeadRepo.DeleteServiceHead(serviceHeadId);
                if (result == -1)
                    return NotFound($"serviceHead with ID {serviceHeadId} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteServiceHead");
                return StatusCode(500, "An error occurred while deleting the serviceHead.");
            }
        }

        [HttpGet("GetServiceHeadMasterExcel")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceHeadMasterExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _serviceHeadRepo.DownloadServiceHeadMasterExcel();
                return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ServiceHeadMaster.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServiceHeadMasterExcel");
                return StatusCode(500, "An error occurred while downloading ServiceHead master Excel.");
            }
        }
    }
}
