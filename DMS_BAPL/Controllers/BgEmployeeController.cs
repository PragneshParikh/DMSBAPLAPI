using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.BgEmployeeMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Controllers
{
    [ApiController]
    [Route("api/BgEmployee")]
    public class BgEmployeeController : ControllerBase
    {
        private readonly IBgEmployeeMasterService _service;

        public BgEmployeeController(IBgEmployeeMasterService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _service.Get());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetEmployeeListView()
            => Ok(await _service.GetEmployeeListView());

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var result = await _service.GetByEmail(email);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BgEmployeeViewModel model)
        {
            try
            {
                var created = await _service.Create(model);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] BgEmployeeViewModel model)
        {
            try
            {
                var rows = await _service.Update(model);
                return rows == 0 ? NotFound() : Ok(new { message = "Employee updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] bool isActive)
        {
            var rows = await _service.UpdateStatus(id, isActive);
            return rows == 0 ? NotFound() : Ok(new { message = "Status updated successfully." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _service.Delete(id);
            return rows == 0 ? NotFound() : Ok(new { message = "Employee deleted successfully." });
        }

        [HttpGet("{id:int}/roles")]
        public async Task<IActionResult> GetRoleMappings(int id)
            => Ok(await _service.GetRoleMappings(id));

        [HttpGet("assigned-dealers")]
        public async Task<IActionResult> GetAssignedDealerCodes([FromQuery] int excludeEmployeeId = 0)
            => Ok(await _service.GetAssignedDealerCodes(excludeEmployeeId));

        [HttpGet("export")]
        public async Task<IActionResult> DownloadExcel()
        {
            var bytes = await _service.DownloadBgEmployeeExcel();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BgEmployees.xlsx");
        }

        // =====================================================
        // TSM ERP INTEGRATION
        // =====================================================
        [HttpPost("TsmEntry")]
        public async Task<IActionResult> ConsumeTsmEntry([FromBody] TsmEntryPayload payload)
        {
            try
            {
                if (payload == null || string.IsNullOrWhiteSpace(payload.TsmCode))
                    return BadRequest(new { message = "tsmcode is required." });

                var result = await _service.ConsumeTsmEntryAsync(payload);
                return Ok(new
                {
                    message = "TSM entry synced successfully.",
                    result.Id,
                    result.TsmCode,
                    result.FirstName,
                    result.LastName,
                    result.EmailId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("TsmEntry/{tsmCode}")]
        public async Task<IActionResult> UpdateTsmEntry(string tsmCode, [FromBody] TsmEntryPayload payload)
        {
            try
            {
                if (payload == null || string.IsNullOrWhiteSpace(tsmCode))
                    return BadRequest(new { message = "tsmcode is required." });

                payload.TsmCode = tsmCode; // route value is source of truth

                var result = await _service.UpdateTsmEntryAsync(payload);
                if (result == null)
                    return NotFound(new { message = $"TSM Code '{tsmCode}' not found. Use POST TsmEntry to create." });

                return Ok(new
                {
                    message = "TSM entry updated successfully.",
                    result.Id,
                    result.TsmCode,
                    result.FirstName,
                    result.LastName,
                    result.EmailId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("TsmEntry/{tsmCode}")]
        public async Task<IActionResult> FetchTsmDetails(string tsmCode)
        {
            var result = await _service.FetchTsmDetailsAsync(tsmCode);
            return result == null ? NotFound() : Ok(result);
        }
    }
}