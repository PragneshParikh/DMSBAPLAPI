using DMS_BAPL_Api.Helpers;
using DMS_BAPL_Data.JobCardScanner;
using DMS_BAPL_Data.Services.B2BJobCardService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// B2B Job Card (24.11.2026) - Angular /b2bjobcards (MenuMaster 160). All data lives in the JobCardScanner database
    /// (connection string "JobCardScannerDb", JobCardScannerDbContext). B2C job cards (api/JobCard, BAPLDMSvad) are not touched.
    ///
    /// Dealer users always work on their own dealer (dealer code from the JWT, mapped to JobCardScanner
    /// dbo.Dealers.BaplDmsDealerCode). Admin / SuperAdmin pass ?dealerCode= (or DealerCode in the body) to pick a dealer.
    ///
    ///   GET  api/b2b-jobcard/ping                                   JobCardScanner connectivity check
    ///   GET  api/b2b-jobcard/dealers                                (Admin / SuperAdmin) JobCardScanner dealers linked to DMS
    ///   GET  api/b2b-jobcard/masters?dealerCode=                    Service Menu rows + complaints + dealer workshops
    ///   GET  api/b2b-jobcard/job-types
    ///   GET  api/b2b-jobcard/service-heads?jobTypeId=
    ///   GET  api/b2b-jobcard/priorities?jobTypeId=&amp;serviceHeadId=
    ///   GET  api/b2b-jobcard/workshops?dealerCode=
    ///   GET  api/b2b-jobcard/staff?dealerCode=&amp;locationCode=     Supervisor/Advisor + Technician of the workshop
    ///   GET  api/b2b-jobcard/complaints
    ///   GET  api/b2b-jobcard/vehicle-suggest?term=   (from 2 chars; caller's dealer first, then every dealer)
    ///   GET  api/b2b-jobcard/vehicle?value=                         vehicle + customer + full service card history
    ///   GET  api/b2b-jobcard?fromDate=&amp;toDate=&amp;jobNo=&amp;chassisNo=&amp;regNo=&amp;customerName=&amp;serviceLocationCode=&amp;status=&amp;pageIndex=&amp;pageSize=
    ///   GET  api/b2b-jobcard/{id}
    ///   POST api/b2b-jobcard                                        create
    ///   POST api/b2b-jobcard/{id}/photos                           check-in photos (blob URLs)
    ///   PUT  api/b2b-jobcard/{id}                                   update (Priority / Expected delivery / Technician)
    /// </summary>
    [Route("api/b2b-jobcard")]
    [ApiController]
    public class B2BJobCardController : ControllerBase
    {
        private readonly IB2BJobCardService _service;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<B2BJobCardController> _logger;

        public B2BJobCardController(IB2BJobCardService service, IConfiguration config, IWebHostEnvironment env, ILogger<B2BJobCardController> logger)
        {
            _service = service;
            _config = config;
            _env = env;
            _logger = logger;
        }

        private B2BCaller Caller()
        {
            var isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
            return new B2BCaller
            {
                UserId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext) ?? "",
                IsAdmin = isAdmin,
                DealerCode = isAdmin ? null : GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)?.Trim()
            };
        }

        private async Task<IActionResult> Run(Func<B2BCaller, Task<IActionResult>> action, string what)
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized(new { success = false, message = "User not authorized" });
            if (string.IsNullOrWhiteSpace(_config.GetConnectionString("JobCardScannerDb")))
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { success = false, message = "Connection string 'JobCardScannerDb' is not configured on the API server." });
            try { return await action(Caller()); }
            catch (B2BValidationException ex) { return BadRequest(new { success = false, message = ex.Message }); }
            catch (B2BNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
            catch (OperationCanceledException) { return StatusCode(499); }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "B2B job card {What}: JobCardScanner database error", what);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"{what} failed (JobCardScanner database): {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B2B job card {What} failed", what);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"{what} failed: {ex.GetBaseException().Message}" });
            }
        }

        [HttpGet("ping")]
        public Task<IActionResult> Ping(CancellationToken ct) =>
            Run(async _ => Ok(await _service.PingAsync(ct)), "Connectivity check");

        [HttpGet("dealers")]
        public Task<IActionResult> Dealers(CancellationToken ct) =>
            Run(async c => c.IsAdmin ? Ok(await _service.GetDealersAsync(ct)) : StatusCode(403, new { success = false, message = "Admin only." }), "Dealer list");

        [HttpGet("masters")]
        public Task<IActionResult> Masters([FromQuery] string? dealerCode, CancellationToken ct) =>
            Run(async c => Ok(await _service.GetMastersAsync(c, dealerCode, ct)), "Masters");

        [HttpGet("job-types")]
        public Task<IActionResult> JobTypes(CancellationToken ct) =>
            Run(async _ => Ok(await _service.GetJobTypesAsync(ct)), "Job types");

        [HttpGet("service-heads")]
        public Task<IActionResult> ServiceHeads([FromQuery] int jobTypeId, CancellationToken ct) =>
            Run(async _ => Ok(await _service.GetServiceHeadsAsync(jobTypeId, ct)), "Service heads");

        [HttpGet("priorities")]
        public Task<IActionResult> Priorities([FromQuery] int jobTypeId, [FromQuery] int serviceHeadId, CancellationToken ct) =>
            Run(async _ => Ok(await _service.GetPrioritiesAsync(jobTypeId, serviceHeadId, ct)), "Priorities");

        [HttpGet("workshops")]
        public Task<IActionResult> Workshops([FromQuery] string? dealerCode, CancellationToken ct) =>
            Run(async c => Ok(await _service.GetWorkshopsAsync(c, dealerCode, ct)), "Workshops");

        [HttpGet("staff")]
        public Task<IActionResult> Staff([FromQuery] string? dealerCode, [FromQuery] string? locationCode, CancellationToken ct) =>
            Run(async c => Ok(await _service.GetStaffAsync(c, dealerCode, locationCode, ct)), "Supervisor / technician list");

        [HttpGet("complaints")]
        public Task<IActionResult> Complaints(CancellationToken ct) =>
            Run(async _ => Ok(await _service.GetComplaintsAsync(ct)), "Complaints");

        [HttpGet("vehicle-suggest")]
        public Task<IActionResult> VehicleSuggest([FromQuery] string? term, [FromQuery] string? dealerCode, CancellationToken ct) =>
            Run(async c => Ok(await _service.SuggestVehiclesAsync(c, term, dealerCode, ct)), "Vehicle search");

        [HttpGet("vehicle")]
        public Task<IActionResult> Vehicle([FromQuery] string? value, [FromQuery] string? dealerCode, CancellationToken ct) =>
            Run(async c => Ok(await _service.GetVehicleAsync(c, value, dealerCode, ct)), "Vehicle details");

        [HttpGet]
        public Task<IActionResult> List([FromQuery] B2BJobCardFilter filter, CancellationToken ct) =>
            Run(async c => Ok(await _service.ListAsync(c, filter ?? new B2BJobCardFilter(), ct)), "B2B job card list");

        [HttpGet("{id:guid}")]
        public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
            Run(async c => Ok(await _service.GetAsync(c, id, ct)), "B2B job card");

        [HttpPost]
        public Task<IActionResult> Create([FromBody] B2BJobCardCreateRequest req, CancellationToken ct) =>
            Run(async c => Ok(await _service.CreateAsync(c, req, ct)), "Save B2B job card");

        /// <summary>
        /// Check-in photos, multipart/form-data: files (1..10 images) + captions (same order, optional). Stored like the
        /// JobCardScanner upload (wwwroot/uploads/jobcard-photos/{jobCardId}/) and saved in JobCardScanner dbo.JobCardPhotos.
        /// </summary>
        [HttpPost("{id:guid}/photos")]
        [RequestSizeLimit(110_000_000)]
        public Task<IActionResult> AddPhotos(Guid id, [FromForm] List<IFormFile> files, [FromForm] List<string>? captions, CancellationToken ct) =>
            Run(async c =>
            {
                files ??= new List<IFormFile>();
                if (files.Count == 0) return BadRequest(new { success = false, message = "No photos to save." });
                var card = await _service.GetAsync(c, id, ct);   // access check (dealer) before anything is written
                if (!card.CanEdit) return BadRequest(new { success = false, message = $"Job card {card.JobCardNumber} is {card.Status}; photos cannot be added." });
                if (card.Photos.Count + files.Count > JobCardPhotoStorage.MaxPhotosPerJobCard)
                    return BadRequest(new { success = false, message = $"A job card can have at most {JobCardPhotoStorage.MaxPhotosPerJobCard} photos." });

                var req = new B2BSavePhotosRequest();
                try
                {
                    for (var i = 0; i < files.Count; i++)
                    {
                        var saved = await JobCardPhotoStorage.SaveAsync(files[i], id.ToString(), Request, _config, _env.ContentRootPath, ct);
                        req.Photos.Add(new B2BPhotoInput
                        {
                            FileName = saved.FileName, BlobUrl = saved.Url, ContentType = saved.ContentType, FileSize = saved.Size,
                            Caption = captions != null && i < captions.Count ? captions[i] : null
                        });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
                return Ok(await _service.AddPhotosAsync(c, id, req, ct));
            }, "Save photos");

        [HttpPut("{id:guid}")]
        public Task<IActionResult> Update(Guid id, [FromBody] B2BJobCardUpdateRequest req, CancellationToken ct) =>
            Run(async c => Ok(await _service.UpdateAsync(c, id, req, ct)), "Update B2B job card");
    }
}
