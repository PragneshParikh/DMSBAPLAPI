using DMS_BAPL_Data.Services.VehicleDispatchService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// Vehicle Dispatch (OEM invoice to dealer = VehicleInward). SuperAdmin only.
    ///   POST api/vehicle-dispatch-data/import/sheets   (file)                  → sheets, row counts, missing columns
    ///   POST api/vehicle-dispatch-data/import          (file, options JSON)    → dry run or real import of one sheet
    ///   POST api/vehicle-dispatch-data/report          (filter JSON body)      → paged report with totals
    /// </summary>
    [Route("api/vehicle-dispatch-data")]
    [ApiController]
    public class VehicleDispatchController : ControllerBase
    {
        private const long MaxUpload = 200L * 1024 * 1024;   // 200 MB per file
        private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

        private readonly IVehicleDispatchService _service;
        private readonly ILogger<VehicleDispatchController> _logger;

        public VehicleDispatchController(IVehicleDispatchService service, ILogger<VehicleDispatchController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private bool IsSuperAdmin() =>
            User?.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals("superadmin", StringComparison.OrdinalIgnoreCase)) == true;

        private IActionResult? Guard(IFormFile? file = null, bool needFile = false)
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            if (!IsSuperAdmin())
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Only SuperAdmin can use Vehicle Dispatch import / report." });
            if (needFile)
            {
                if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "Choose an Excel file." });
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".xlsx" && ext != ".xlsm")
                    return BadRequest(new { success = false, message = "Only .xlsx / .xlsm files are supported. Open .xls / .csv in Excel and Save As .xlsx." });
            }
            return null;
        }

        [HttpPost("import/sheets")]
        [RequestSizeLimit(MaxUpload)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUpload)]
        public IActionResult Sheets(IFormFile file)
        {
            var denied = Guard(file, needFile: true);
            if (denied != null) return denied;
            try
            {
                using var stream = file.OpenReadStream();
                return Ok(_service.ReadSheets(stream, file.FileName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle dispatch: could not read {File}", file.FileName);
                return StatusCode(500, new { success = false, message = "Could not read the Excel file: " + ex.GetBaseException().Message });
            }
        }

        [HttpPost("import")]
        [RequestSizeLimit(MaxUpload)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUpload)]
        public async Task<IActionResult> Import(IFormFile file, [FromForm] string? options)
        {
            var denied = Guard(file, needFile: true);
            if (denied != null) return denied;
            try
            {
                var opts = string.IsNullOrWhiteSpace(options)
                    ? new VehicleDispatchImportOptions()
                    : JsonSerializer.Deserialize<VehicleDispatchImportOptions>(options, Json) ?? new VehicleDispatchImportOptions();
                var userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                using var stream = file.OpenReadStream();
                return Ok(await _service.ImportAsync(stream, file.FileName, opts, userId));
            }
            catch (JsonException)
            {
                return BadRequest(new { success = false, message = "Import options are not valid JSON." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle dispatch import failed for {File}", file.FileName);
                return StatusCode(500, new { success = false, message = "Import failed (nothing saved): " + ex.GetBaseException().Message });
            }
        }

        [HttpPost("report")]
        [ProducesResponseType(typeof(VehicleDispatchReportResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Report([FromBody] VehicleDispatchReportFilter filter)
        {
            var denied = Guard();
            if (denied != null) return denied;
            try
            {
                return Ok(await _service.GetReportAsync(filter));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle dispatch report failed");
                return StatusCode(500, new { success = false, message = "Could not load the report: " + ex.GetBaseException().Message });
            }
        }
    }
}
