using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.ServiceHeadRepo;
using DMS_BAPL_Data.Repositories.ServiceTypeRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceTypeMasterController : ControllerBase
    {
        private readonly IServiceTypeMasterRepo _serviceTypeMasterRepo;
        private readonly ILogger<ServiceTypeMasterController> _logger;

        public ServiceTypeMasterController(IServiceTypeMasterRepo serviceTypeMasterRepo, ILogger<ServiceTypeMasterController> logger)
        {
            _serviceTypeMasterRepo = serviceTypeMasterRepo;
            _logger = logger;
        }

        [HttpGet("GetAllServiceType")]
        [ProducesResponseType(typeof(List<ServiceTypeMasterViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllServiceType()
        {
            try
            {
                var result = await _serviceTypeMasterRepo.GetAllServiceType();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllServiceType");
                return StatusCode(500, "An error occurred while retrieving ServiceType.");
            }
        }

        [HttpPost("InsertserviceType")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertserviceType(ServiceTypeMasterViewModel serviceTypeMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _serviceTypeMasterRepo.InsertserviceType(serviceTypeMasterViewModel, userId);
                if (result == -1)
                    return StatusCode(500, "An error occurred while inserting the serviceType.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Insert serviceType");
                return StatusCode(500, "An error occurred while inserting the serviceType.");
            }
        }
        [HttpPut("UpdateserviceTypeName")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateserviceTypeName([FromBody] ServiceTypeMasterViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _serviceTypeMasterRepo.UpdateserviceTypeName(model, userId);
                if (result == -1)
                    return NotFound($"ServiceType with ID {model.Id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateServiceTypeName");
                return StatusCode(500, "An error occurred while updating the ServiceType name.");
            }
        }

        [HttpDelete("DeleteserviceType/{serviceTypeId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteserviceType(int serviceTypeId)
        {
            try
            {
                var result = await _serviceTypeMasterRepo.DeleteserviceType(serviceTypeId);
                if (result == -1)
                    return NotFound($"serviceType with ID {serviceTypeId} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteServiceType");
                return StatusCode(500, "An error occurred while deleting the serviceType.");
            }
        }

        [HttpGet("GetServiceTypeMasterExcel")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceTypeMasterExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _serviceTypeMasterRepo.DownloadserviceTypeMasterExcel();
                return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ServiceTypeMaster.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServiceTypeMasterExcel");
                return StatusCode(500, "An error occurred while downloading SeriveType master Excel.");
            }
        }

    }
}
