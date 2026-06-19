using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.JobSourceMasterRepo;
using DMS_BAPL_Data.Repositories.JobTypeMasterRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobSourceController : ControllerBase
    {
        private readonly IJobSourceMasterRepo _jobSourceMasterRepo;
        private readonly ILogger<JobSourceController> _logger;

        public JobSourceController(IJobSourceMasterRepo jobSourceMasterRepo, ILogger<JobSourceController> logger)
        {
            _jobSourceMasterRepo = jobSourceMasterRepo;
            _logger = logger;
        }

        [HttpGet("GetAllJobSource")]
        [ProducesResponseType(typeof(List<JobSourceMasterViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllJobSource()
        {
            try
            {
                var result = await _jobSourceMasterRepo.GetAllJobSource();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllJobSource");
                return StatusCode(500, "An error occurred while retrieving JobSource.");
            }
        }

        [HttpPost("InsertJobSource")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertJobSource(JobSourceMasterViewModel jobSourceMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _jobSourceMasterRepo.InsertJobSource(jobSourceMasterViewModel, userId);
                if (result == -1)
                    return StatusCode(500, "An error occurred while inserting the JobSource.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertJobSource");
                return StatusCode(500, "An error occurred while inserting the JobSource.");
            }
        }
        [HttpPut("UpdateJobSourceName")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateJobSourceName([FromBody] JobSourceMasterViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _jobSourceMasterRepo.UpdateJobSourceName(model, userId);
                if (result == -1)
                    return NotFound($"JobSource with ID {model.Id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateJobSourceName");
                return StatusCode(500, "An error occurred while updating the JobSource name.");
            }
        }

        [HttpDelete("DeleteJobSource/{jobSourceId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteJobSource(int jobSourceId)
        {
            try
            {
                var result = await _jobSourceMasterRepo.DeleteJobSource(jobSourceId);
                if (result == -1)
                    return NotFound($"JobSource with ID {jobSourceId} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteJobSource");
                return StatusCode(500, "An error occurred while deleting the JobSource.");
            }
        }

        [HttpGet("GetJobSourceMasterExcel")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobSourceMasterExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _jobSourceMasterRepo.DownloadjobsourceMasterExcel();
                return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "JobSourceMaster.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetJobSourceMasterExcel");
                return StatusCode(500, "An error occurred while downloading JobSource master Excel.");
            }
        }

    }
}
