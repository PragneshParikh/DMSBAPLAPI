using DMS_BAPL_Data.Services.JobCardServiceHistoryImportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// Jobcard Servicehistory Excel → BAPLDMSvad.dbo.JobCardServiceHistory (Utility menu). Admin / SuperAdmin only.
    ///   GET    api/jobcard-servicehistory-import/fields
    ///   POST   api/jobcard-servicehistory-import/upload            (file)             → uploadId, headers, mapping, sample rows, destination status
    ///   POST   api/jobcard-servicehistory-import/{uploadId}/run    (JcshRunOptions)   → starts background validate (dryRun) / import job
    ///   GET    api/jobcard-servicehistory-import/jobs/{jobId}                         → progress + counts + row-wise issues
    ///   POST   api/jobcard-servicehistory-import/jobs/{jobId}/cancel
    ///   DELETE api/jobcard-servicehistory-import/{uploadId}                           → remove the temp file
    /// </summary>
    [Route("api/jobcard-servicehistory-import")]
    [ApiController]
    public class JobCardServiceHistoryImportController : ControllerBase
    {
        private const long MaxUpload = 200L * 1024 * 1024;   // 200 MB per file (same as Vehicle Sale Import)

        private readonly IJobCardServiceHistoryImportService _service;
        private readonly ILogger<JobCardServiceHistoryImportController> _logger;

        public JobCardServiceHistoryImportController(IJobCardServiceHistoryImportService service, ILogger<JobCardServiceHistoryImportController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private IActionResult? Guard()
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Only Admin / SuperAdmin can import job card service history." });
            return null;
        }

        [HttpGet("fields")]
        public IActionResult Fields() => Guard() ?? Ok(_service.GetFields());

        [HttpPost("upload")]
        [RequestSizeLimit(MaxUpload)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUpload)]
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
        {
            var denied = Guard();
            if (denied != null) return denied;
            if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "Choose an Excel file." });
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xlsm")
                return BadRequest(new { success = false, message = "Only .xlsx / .xlsm files are supported. Open .xls / .csv in Excel and Save As .xlsx." });
            try
            {
                await using var stream = file.OpenReadStream();
                return Ok(await _service.UploadAsync(stream, file.FileName, GetUserInfoFromToken.GetUserIdFromToken(HttpContext), ct));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Jobcard servicehistory upload failed for {File}", file.FileName);
                return StatusCode(500, new { success = false, message = "Could not read the Excel file: " + ex.GetBaseException().Message });
            }
        }

        [HttpPost("{uploadId:guid}/run")]
        public IActionResult Run(Guid uploadId, [FromBody] JcshRunOptions options)
        {
            var denied = Guard();
            if (denied != null) return denied;
            try
            {
                return Ok(_service.Start(uploadId, options ?? new JcshRunOptions(), GetUserInfoFromToken.GetUserIdFromToken(HttpContext)));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { success = false, message = ex.Message }); }
        }

        [HttpGet("jobs/{jobId:guid}")]
        public IActionResult Job(Guid jobId)
        {
            var denied = Guard();
            if (denied != null) return denied;
            var job = _service.GetJob(jobId);
            return job == null ? NotFound(new { success = false, message = "Job not found (API restarted?). Start validation again." }) : Ok(job);
        }

        [HttpPost("jobs/{jobId:guid}/cancel")]
        public IActionResult Cancel(Guid jobId)
        {
            var denied = Guard();
            if (denied != null) return denied;
            var job = _service.Cancel(jobId);
            return job == null ? NotFound(new { success = false, message = "Job not found." }) : Ok(job);
        }

        [HttpDelete("{uploadId:guid}")]
        public IActionResult Discard(Guid uploadId)
        {
            var denied = Guard();
            if (denied != null) return denied;
            _service.Discard(uploadId, GetUserInfoFromToken.GetUserIdFromToken(HttpContext));
            return NoContent();
        }
    }

    /// <summary>
    /// Read API for the imported service history (BAPLDMSvad.dbo.JobCardServiceHistory). Admin / SuperAdmin only.
    ///   GET api/jobcard-servicehistory?chassisNo=&amp;regNo=&amp;dealerCode=&amp;jobNo=&amp;fromDate=&amp;toDate=&amp;page=1&amp;pageSize=50
    ///   GET api/jobcard-servicehistory/chassis/{chassisNo}   (latest 500 jobs of one vehicle)
    /// </summary>
    [Route("api/jobcard-servicehistory")]
    [ApiController]
    public class JobCardServiceHistoryController : ControllerBase
    {
        private readonly IJobCardServiceHistoryReadService _read;
        private readonly ILogger<JobCardServiceHistoryController> _logger;

        public JobCardServiceHistoryController(IJobCardServiceHistoryReadService read, ILogger<JobCardServiceHistoryController> logger)
        {
            _read = read;
            _logger = logger;
        }

        private IActionResult? Guard()
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Only Admin / SuperAdmin can view imported service history." });
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] JcshHistoryQuery q, CancellationToken ct)
        {
            var denied = Guard();
            if (denied != null) return denied;
            try { return Ok(await _read.SearchAsync(q, ct)); }
            catch (InvalidOperationException ex) { return StatusCode(503, new { success = false, message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service history search failed");
                return StatusCode(500, new { success = false, message = "Could not read service history: " + ex.GetBaseException().Message });
            }
        }

        [HttpGet("chassis/{chassisNo}")]
        public async Task<IActionResult> ByChassis(string chassisNo, CancellationToken ct)
        {
            var denied = Guard();
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(chassisNo)) return BadRequest(new { success = false, message = "Chassis No is required." });
            try { return Ok(await _read.ByChassisAsync(chassisNo, ct)); }
            catch (InvalidOperationException ex) { return StatusCode(503, new { success = false, message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service history by chassis failed");
                return StatusCode(500, new { success = false, message = "Could not read service history: " + ex.GetBaseException().Message });
            }
        }
    }
}
