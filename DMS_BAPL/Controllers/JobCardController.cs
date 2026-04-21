using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Utils.Constants;
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

        public JobCardController(IJobCardRepo jobCardRepo)
        {
            _jobCardRepo = jobCardRepo;
        }

        [HttpGet("GetJobType")]
        public async Task<IActionResult> GetJobType()
        {
            var jobTypes = await _jobCardRepo.GetJobtype();
            return Ok(jobTypes);
        }

        [HttpGet("GetServiceDataByJobType")]
        public async Task<IActionResult> GetServiceDataByJobType(string jobTypeName)
        {
            var serviceData = await _jobCardRepo.GetServiceDataByJobType(jobTypeName);
            return Ok(serviceData);
        }

        [HttpGet("GetServiceHead")]
        public async Task<IActionResult> GetServiceHead(int jobTypeId)
        {
            var serviceHeads = await _jobCardRepo.GetServiceHead(jobTypeId);
            return Ok(serviceHeads);
        }

        [HttpGet("GetServiceType")]
        public async Task<IActionResult> GetServiceType(int serviceHeadId)
        {
            var serviceTypes = await _jobCardRepo.GetServiceType(serviceHeadId);
            return Ok(serviceTypes);
        }

        [HttpGet("GetAllInspectedChassis")]
        public async Task<IActionResult> GetAllInspectedChassis(string dealerCode)
        {
            var data = await _jobCardRepo.GetAllInspectedLotChassisAsync(dealerCode);

            if (data == null || !data.Any())
            {
                return NotFound("No inspected records found.");
            }

            return Ok(data);
        }
        [HttpGet("GetJobSource")]
        public async Task<IActionResult> GetJobSource()
        {
            var jobSources = await _jobCardRepo.GetJobSource();
            return Ok(jobSources);
        }

        [HttpGet("GetPdiChecklist")]
        public async Task<IActionResult> GetPdiChecklist()
        {
            var checklist = await _jobCardRepo.GetPdichecklist();
            return Ok(checklist);
        }

        
        [HttpGet("GetJobCardList")]
        public async Task<IActionResult> GetJobCardList(string dealerCode)
        {
            var jobCardList = await _jobCardRepo.GetJobCardListViewAsync(dealerCode);
            if (jobCardList == null || !jobCardList.Any())
            {
                return NotFound(StringConstants.JobCardNotFound);
            }
            return Ok(jobCardList);
        }

        [HttpPost("SaveJobCardDetails")]
        public async Task<IActionResult> SaveJobCardDetails(JobCardDetailsViewModel jobCardDetailsView)
        {
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

        [HttpPut("UpdateJobCardDetails")]
        public async Task<IActionResult> UpdateJobCardDetails([FromBody] UpdateJobCardVM updateJobCardDetails)
        {
            if (updateJobCardDetails == null)
                return BadRequest("Invalid data");

            var result = await _jobCardRepo.UpdateJobCardinfoDetails(updateJobCardDetails);

            if (result > 0)
                return Ok(new { message = "Job card updated successfully" });

            return NotFound(new { message = "Job card not found" });
        }

    }
}
