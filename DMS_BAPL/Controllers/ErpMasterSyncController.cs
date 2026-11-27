using System.Security.Claims;
using DMS_BAPL_Data.Services.ErpMasterSyncService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// 26.11.2026 - Dealer Master / Location Master: UAT ERP report import + ERP sync (SuperAdmin only).
    /// Every call is a preview unless apply=true (the UI shows the preview, then confirms with apply=true).
    ///   POST api/erp-master-sync/dealers/import?apply=false       multipart "file" = UAT_ERP_ManageDealerReport
    ///   POST api/erp-master-sync/dealers/sync?apply=false&amp;dealerCode=      ERP C_CustomerMaster
    ///   POST api/erp-master-sync/locations/import?apply=false&amp;inactivateMissing=true   multipart "file" = UAT_ERP_LocatonList
    ///   POST api/erp-master-sync/locations/sync?apply=false&amp;dealerCode=    ERP C_DMS_DealerConsigneeDetail
    /// </summary>
    [Route("api/erp-master-sync")]
    [ApiController]
    public class ErpMasterSyncController : ControllerBase
    {
        private readonly IErpMasterSyncService _service;
        private readonly ILogger<ErpMasterSyncController> _logger;

        public ErpMasterSyncController(IErpMasterSyncService service, ILogger<ErpMasterSyncController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private bool IsSuperAdmin =>
            User?.FindAll(ClaimTypes.Role).Any(c => string.Equals(c.Value, "superadmin", StringComparison.OrdinalIgnoreCase)) == true;

        private async Task<IActionResult> Run(Func<string, Task<ErpSyncResult>> action, string what)
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
            if (User?.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authorized" });
            if (!IsSuperAdmin)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only SuperAdmin can import / sync dealers and locations." });
            try
            {
                return Ok(await action(userId));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "{What} failed (database)", what);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"{what} failed (database): {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{What} failed", what);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"{what} failed: {ex.GetBaseException().Message}" });
            }
        }

        [HttpPost("dealers/import")]
        [RequestSizeLimit(50_000_000)]
        public Task<IActionResult> ImportDealers(IFormFile file, [FromQuery] bool apply, CancellationToken ct) =>
            Run(u => _service.ImportUatDealersAsync(file, apply, u, ct), "Dealer import");

        [HttpPost("dealers/sync")]
        public Task<IActionResult> SyncDealers([FromQuery] bool apply, [FromQuery] string? dealerCode, CancellationToken ct) =>
            Run(u => _service.SyncDealersFromErpAsync(dealerCode, apply, u, ct), "Dealer ERP sync");

        [HttpPost("locations/import")]
        [RequestSizeLimit(50_000_000)]
        public Task<IActionResult> ImportLocations(IFormFile file, [FromQuery] bool apply, [FromQuery] bool inactivateMissing = true, CancellationToken ct = default) =>
            Run(u => _service.ImportUatLocationsAsync(file, apply, inactivateMissing, u, ct), "Location import");

        [HttpPost("locations/sync")]
        public Task<IActionResult> SyncLocations([FromQuery] bool apply, [FromQuery] string? dealerCode, CancellationToken ct) =>
            Run(u => _service.SyncLocationsFromErpAsync(dealerCode, apply, u, ct), "Location ERP sync");
    }
}
