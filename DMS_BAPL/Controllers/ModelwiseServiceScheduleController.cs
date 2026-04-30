using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.ModelWiseServieScheduleRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelwiseServiceScheduleController : ControllerBase
    {
        private readonly IModelwiseServiceSchedule _modelwiseServiceSchedule;
        private readonly ILogger<ModelwiseServiceScheduleController> _logger;
        public ModelwiseServiceScheduleController(IModelwiseServiceSchedule modelwiseServiceSchedule, ILogger<ModelwiseServiceScheduleController> logger)
        {
            _modelwiseServiceSchedule = modelwiseServiceSchedule;
            _logger = logger;
        }
        [HttpGet("GetServiceHead")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceHead()
        {
            try
            {
                _logger.LogInformation("GetServiceHead method called.");
                var result = await _modelwiseServiceSchedule.GetServiceHeadViews();
                return Ok(result);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetServiceHead method.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

        }
        // insert update maange
        [HttpPost("SavemodelwiseserviceSchedule")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SavemodelwiseserviceSchedule(List<ServiceScheduleVM> serviceScheduleVM)
        {
            try
            {
                _logger.LogInformation("SavemodelwiseserviceSchedule method called.");
                var result = await _modelwiseServiceSchedule.SavemodelwiseserviceScheduleAsync(serviceScheduleVM);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in SavemodelwiseserviceSchedule method.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // get list 
        [HttpGet("GetmodelwiseserviceSchedulelist")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelwiseservicescheduleList(int? oemModelId, DateTime? effectiveDate)
        {
            try
            {
                _logger.LogInformation("GetmodelwiseserviceSchedule method called.");
                var result = await _modelwiseServiceSchedule.GetModelwiseservicescheduleListAsync(oemModelId, effectiveDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetmodelwiseserviceSchedule method.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        //for edit time get value
        [HttpGet("GetByModelwiseserviceschedule")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByModelwiseserviceschedule(int oemModelId)
        {
            try
            {
                _logger.LogInformation("GetByModelwiseserviceschedule method called.");
                var result = await _modelwiseServiceSchedule.GetByModelwiseservicescheduleAsync(oemModelId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetByModelwiseserviceschedule method.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        [HttpGet("GetOemModelbasedModelVarientList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOemModelbasedModelVarientList(int oemModelId)
        {
            try
            {
                _logger.LogInformation("GetOemModelbasedModelVarientList method called.");
                var result = await _modelwiseServiceSchedule.GetOemModelbasedModelVarientListAsync(oemModelId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetOemModelbasedModelVarientList method.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
