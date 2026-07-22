using DMS_BAPL_Data.Services.ZoneMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZoneMasterController : ControllerBase
    {
        private readonly IZoneMasterService _service;

        public ZoneMasterController(IZoneMasterService service)
            => _service = service;

        // GET api/ZoneMaster/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _service.GetAllZonesAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET api/ZoneMaster/GetDealersByZone/{zone}
        [HttpGet("GetDealersByZone/{zone}")]
        public async Task<IActionResult> GetDealersByZone(string zone)
        {
            try { return Ok(await _service.GetDealersByZoneAsync(zone)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET api/ZoneMaster/GetById/{id}
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetZoneByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"Zone {id} not found." });
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/ZoneMaster/Save
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] ZoneMasterViewModel model)
        {
            try { return Ok(await _service.CreateZoneAsync(model)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // PUT api/ZoneMaster/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ZoneMasterViewModel model)
        {
            try
            {
                model.Id = id;
                var ok = await _service.UpdateZoneAsync(model);
                if (!ok) return NotFound(new { message = $"Zone {id} not found." });
                return Ok(new { message = "Updated." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // DELETE api/ZoneMaster/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _service.DeleteZoneAsync(id);
                if (!ok) return NotFound(new { message = $"Zone {id} not found." });
                return Ok(new { message = "Deactivated." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}

// ── Program.cs registrations ─────────────────────────────
// builder.Services.AddScoped<IZoneMasterRepo,    ZoneMasterRepo>();
// builder.Services.AddScoped<IZoneMasterService, ZoneMasterService>();