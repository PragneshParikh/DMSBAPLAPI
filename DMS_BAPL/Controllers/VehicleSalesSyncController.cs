using DMS_BAPL_Data.Services;
using DMS_BAPL_Data.Services.VehicleSalesService;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleSalesSyncController : ControllerBase
{
    private readonly IErpVsrSyncService _sync;
    public VehicleSalesSyncController(IErpVsrSyncService sync) => _sync = sync;

    [HttpPost("today")]
    public async Task<IActionResult> SyncToday()
    {
        var (f, i, u) = await _sync.SyncVsrForDateAsync(DateTime.UtcNow.Date);
        return Ok(new { fetched = f, inserted = i, updated = u });
    }

    [HttpPost("range")]
    public async Task<IActionResult> SyncRange([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var f) || !DateTime.TryParse(to, out var t))
            return BadRequest("Use yyyy-MM-dd for both dates.");

        var (fetched, inserted, updated) = await _sync.SyncVsrForRangeAsync(f, t);
        return Ok(new { fetched, inserted, updated });
    }
}