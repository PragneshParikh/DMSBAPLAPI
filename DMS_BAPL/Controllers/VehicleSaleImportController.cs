using DMS_BAPL_Data.Services.VehicleSaleImportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// Cogxim (old DMS) sale register Excel → VehicleSaleBillHeader / VehicleSaleBillDetails.
    /// Admin / SuperAdmin only.
    ///   GET  api/vehicle-sale-import/fields    target fields + aliases
    ///   POST api/vehicle-sale-import/preview   (file, sheetName?, headerRow?)  → headers, auto-mapping, sample rows
    ///   POST api/vehicle-sale-import/import    (file, options JSON)            → dry run or real import result
    /// </summary>
    [Route("api/vehicle-sale-import")]
    [ApiController]
    public class VehicleSaleImportController : ControllerBase
    {
        private const long MaxUpload = 200L * 1024 * 1024;   // 200 MB per file
        private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

        private readonly IVehicleSaleImportService _service;
        private readonly ILogger<VehicleSaleImportController> _logger;

        public VehicleSaleImportController(IVehicleSaleImportService service, ILogger<VehicleSaleImportController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private IActionResult? Guard(IFormFile? file = null, bool needFile = false)
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Only Admin / SuperAdmin can import sale data." });
            if (needFile)
            {
                if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "Choose an Excel file." });
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".xlsx" && ext != ".xlsm")
                    return BadRequest(new { success = false, message = "Only .xlsx / .xlsm files are supported. Open .xls / .csv in Excel and Save As .xlsx." });
            }
            return null;
        }

        [HttpGet("fields")]
        public IActionResult Fields()
        {
            var denied = Guard();
            if (denied != null) return denied;
            return Ok(_service.GetFields());
        }

        [HttpPost("preview")]
        [RequestSizeLimit(MaxUpload)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUpload)]
        public IActionResult Preview(IFormFile file, [FromForm] string? sheetName, [FromForm] int headerRow = 1)
        {
            var denied = Guard(file, needFile: true);
            if (denied != null) return denied;
            try
            {
                using var stream = file.OpenReadStream();
                return Ok(_service.Preview(stream, file.FileName, sheetName, headerRow));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cogxim preview failed for {File}", file.FileName);
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
                    ? new VehicleSaleImportOptions()
                    : JsonSerializer.Deserialize<VehicleSaleImportOptions>(options, Json) ?? new VehicleSaleImportOptions();

                var userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                using var stream = file.OpenReadStream();
                var result = await _service.ImportAsync(stream, file.FileName, opts, userId);
                return Ok(result);
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
                _logger.LogError(ex, "Cogxim import failed for {File}", file.FileName);
                return StatusCode(500, new { success = false, message = "Import failed: " + ex.GetBaseException().Message });
            }
        }
    }
}
