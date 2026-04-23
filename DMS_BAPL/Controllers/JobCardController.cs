using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobCardController : ControllerBase
    {
        private readonly IJobCardRepo _jobCardRepo;
        private readonly ILogger<LOTInspectionController> _logger;

        public JobCardController(IJobCardRepo jobCardRepo)
        {
            _jobCardRepo = jobCardRepo;
        }

        [HttpGet("GetJobType")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobType()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var jobTypes = await _jobCardRepo.GetJobtype();
                return Ok(jobTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetJobType");
                return StatusCode(500, "An error occurred while fetching job types.");
            }

        }

        [HttpGet("GetServiceDataByJobType")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceDataByJobType(string jobTypeName)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var serviceData = await _jobCardRepo.GetServiceDataByJobType(jobTypeName);
                return Ok(serviceData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServiceDataByJobType");
                return StatusCode(500, "An error occurred while fetching service data.");
            }
        }
        [HttpGet("GetServiceHead")]
        public async Task<IActionResult> GetServiceHead(int jobTypeId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var serviceHeads = await _jobCardRepo.GetServiceHead(jobTypeId);
                return Ok(serviceHeads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServiceHead");
                return StatusCode(500, "An error occurred while fetching service heads.");
            }
        }

        [HttpGet("GetServiceType")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceType(int serviceHeadId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var serviceTypes = await _jobCardRepo.GetServiceType(serviceHeadId);
                return Ok(serviceTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServiceType");
                return StatusCode(500, "An error occurred while fetching service types.");
            }
        }
        [HttpGet("GetAllInspectedChassis")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllInspectedChassis(string dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var data = await _jobCardRepo.GetAllInspectedLotChassisAsync(dealerCode);

                if (data == null || !data.Any())
                {
                    return NotFound("No inspected records found.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllInspectedChassis");
                return StatusCode(500, "An error occurred while fetching inspected chassis.");
            }
        }
        [HttpGet("GetJobSource")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobSource()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var jobSources = await _jobCardRepo.GetJobSource();
                return Ok(jobSources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetJobSource");
                return StatusCode(500, "An error occurred while fetching job sources.");
            }
        }
        [HttpGet("GetPdiChecklist")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPdiChecklist()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var checklist = await _jobCardRepo.GetPdichecklist();
                return Ok(checklist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPdiChecklist");
                return StatusCode(500, "An error occurred while fetching PDI checklist.");
            }
        }
        [HttpGet("GetJobCardList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardList(string dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var jobCardList = await _jobCardRepo.GetJobCardListViewAsync(dealerCode);
                if (jobCardList == null || !jobCardList.Any())
                {
                    return NotFound(StringConstants.JobCardNotFound);
                }
                return Ok(jobCardList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetJobCardList");
                return StatusCode(500, "An error occurred while fetching job card list.");
            }
        }

        [HttpPost("SaveJobCardDetails")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveJobCardDetails(JobCardDetailsViewModel jobCardDetailsView)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _jobCardRepo.InsertJobCardinfoDetails(jobCardDetailsView);
                if (result > 0)
                {
                    return Ok(new
                    {
                        message = StringConstants.JobCardDetailsSaved
                    });
                }
                else
                {
                    return StatusCode(500, "An error occurred while saving job card details.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveJobCardDetails");
                return StatusCode(500, "An error occurred while saving job card details.");
            }
        }

        [HttpPut("UpdateJobCardDetails")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateJobCardDetails([FromBody] UpdateJobCardVM updateJobCardDetails)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (updateJobCardDetails == null)
                    return BadRequest("Invalid data");

                var result = await _jobCardRepo.UpdateJobCardinfoDetails(updateJobCardDetails);

                if (result > 0)
                    return Ok(new { message = "Job card updated successfully" });

                return NotFound(new { message = "Job card not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateJobCardDetails");
                return StatusCode(500, "An error occurred while updating job card details.");
            }
        }

        //[HttpGet("{Id}")]
        //[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetJobCardDetailsById(int Id)
        //{
        //    try
        //    {
        //        string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

        //        if (string.IsNullOrEmpty(userId))
        //            return Unauthorized("User not authorized");

        //        var jobCard = _jobCardRepo.GetJobCardById(Id);

        //        return Ok(jobCard);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        [HttpGet("GetFilteredJobCard")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<object>>> GetFilterJobCardDetails(
            [FromQuery] int pageSize,
            [FromQuery] int pageIndex,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? jobNo,
            [FromQuery] int? manualJobNo)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var jobCards = await _jobCardRepo.GetFilterdJobCardDetails(fromDate, toDate, jobNo, manualJobNo, pageIndex, pageSize);

                return Ok(jobCards);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpDelete("DeleteJobCard/{id}/{role}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteJobCard(int jobId, string role)
        {
            try
            {
                if (role != "SuperAdmin")
                    return Unauthorized("Only Super Admin can delete");

                var result = await _jobCardRepo.DeleteJobCard(jobId);

                if (result > 0)
                    return Ok(new { message = "Deleted Successfully" });

                return NotFound("Job Card not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteJobCard");
                return StatusCode(500, "An error occurred while deleting the job card.");
            }
            
        }

        [HttpPost("SearchJobCard")]
        public async Task<IActionResult> SearchJobCard([FromBody] JobCardSearchModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _jobCardRepo.SearchJobCards(model);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchJobCard");
                return StatusCode(500, "An error occurred while searching job cards.");
            }
        }

    }
}
