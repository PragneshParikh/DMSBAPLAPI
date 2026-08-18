using DMS_BAPL_Data.Services.DispatchMasterService;
using DMS_BAPL_Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DMS_BAPL_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DispatchMasterController : ControllerBase
    {
        private readonly IDispatchMasterService _service;

        public DispatchMasterController(IDispatchMasterService service)
        {
            _service = service;
        }

        // POST: api/DispatchMaster/Search
        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] DispatchMasterSearchViewModel searchModel)
        {
            var (data, totalRecords) = await _service.GetAllAsync(searchModel);
            return Ok(new
            {
                Success = true,
                TotalRecords = totalRecords,
                Data = data
            });
        }

        // GET: api/DispatchMaster/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { Success = false, Message = "Record not found." });

            return Ok(new { Success = true, Data = result });
        }

        // POST: api/DispatchMaster/Save
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] DispatchMasterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.UpdatedBy = User.FindFirstValue(ClaimTypes.Name) ?? "System";

            var (success, message) = await _service.SaveAsync(model);
            if (!success)
                return BadRequest(new { Success = false, Message = message });

            return Ok(new { Success = true, Message = message });
        }

        // PUT: api/DispatchMaster/ToggleActive/5?isActive=true
        [HttpPut("ToggleActive/{id:int}")]
        public async Task<IActionResult> ToggleActive(int id, [FromQuery] bool isActive)
        {
            var (success, message) = await _service.ToggleActiveAsync(id, isActive);
            if (!success)
                return NotFound(new { Success = false, Message = message });

            return Ok(new { Success = true, Message = message });
        }

        // DELETE: api/DispatchMaster/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { Success = false, Message = message });

            return Ok(new { Success = true, Message = message });
        }
    }
}