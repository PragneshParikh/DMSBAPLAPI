using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.GroupMasterRepo;
using DMS_BAPL_Data.Repositories.JobTypeMasterRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTypeController : ControllerBase
    {
       
            private readonly IJobTypeMasterRepo _jobTypeMasterRepo;
            private readonly ILogger<JobTypeController> _logger;

            public JobTypeController(IJobTypeMasterRepo jobTypeMasterRepo, ILogger<JobTypeController> logger)
            {
                _jobTypeMasterRepo = jobTypeMasterRepo;
                _logger = logger;
            }

            [HttpGet("GetAllJobType")]
            [ProducesResponseType(typeof(List<JobTypeMasterViewModel>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> GetAllJobType()
            {
                try
                {
                    var result = await _jobTypeMasterRepo.GetAllJobType();
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetAllJobType");
                    return StatusCode(500, "An error occurred while retrieving groups.");
                }
            }

            [HttpPost("InsertJobType")]
            [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> InsertJobType(JobTypeMasterViewModel jobtypeMasterViewModel)
            {
                try
                {
                    string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized("User not authorized");
                    var result = await _jobTypeMasterRepo.InsertJobType(jobtypeMasterViewModel, userId);
                    if (result == -1)
                        return StatusCode(500, "An error occurred while inserting the JobType.");
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in InsertJobType");
                    return StatusCode(500, "An error occurred while inserting the JobType.");
                }
            }
            [HttpPut("UpdateJobTypeName")]
            [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> UpdateJobTypeName([FromBody] JobTypeMasterViewModel model)
            {
                try
                {
                    string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized("User not authorized");
                    var result = await _jobTypeMasterRepo.UpdateJobTypeName(model, userId);
                    if (result == -1)
                        return NotFound($"JobType with ID {model.Id} not found.");
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in UpdateJobTypeName");
                    return StatusCode(500, "An error occurred while updating the Jobtype name.");
                }
            }

            [HttpDelete("DeleteJobType/{jobTypeId}")]
            [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> DeleteJobType(int jobTypeId)
            {
                try
                {
                    var result = await _jobTypeMasterRepo.DeleteJobType(jobTypeId);
                    if (result == -1)
                        return NotFound($"JobType with ID {jobTypeId} not found.");
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DeleteJobType");
                    return StatusCode(500, "An error occurred while deleting the JobType.");
                }
            }

            [HttpGet("GetJobTypeMasterExcel")]
            [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> GetJobTypeMasterExcel()
            {
                try
                {
                    string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized("User not authorized");
                    var result = await _jobTypeMasterRepo.DownloadJobTypeMasterExcel();
                    return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "JobTypeMaster.xlsx");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetJobTypeMasterExcel");
                    return StatusCode(500, "An error occurred while downloading JobType master Excel.");
                }
            }
        }


    }

