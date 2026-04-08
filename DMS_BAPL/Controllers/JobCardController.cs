using DMS_BAPL_Data.Repositories.JobCardRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
