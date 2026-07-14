using DMS_BAPL_Data.Services.BgEmployeeMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BgEmployeeController : ControllerBase
    {
        private readonly IBgEmployeeMasterService _service;

        public BgEmployeeController(IBgEmployeeMasterService service)
        {
            _service = service;
        }

        // =====================================================
        // TSM ERP LOOKUP (GET, pull) — proxies external ERP API
        // GET api/BgEmployee/TsmLookup/{tsmCode}
        // =====================================================

        [HttpGet("TsmLookup/{tsmCode}")]
        public async Task<IActionResult> TsmLookup(string tsmCode)
        {
            try
            {
                var result = await _service.FetchTsmDetailsAsync(tsmCode);

                if (result == null)
                    return NotFound(new { message = $"TSM Code '{tsmCode}' not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── TEMPORARY DIAGNOSTIC — remove once real route confirmed
        [HttpGet("TsmLookupRaw/{tsmCode}")]
        public async Task<IActionResult> TsmLookupRaw(string tsmCode)
        {
            try
            {
                var attempts = await _service.FetchTsmRawAsync(tsmCode);

                var sb = new System.Text.StringBuilder();
                foreach (var (label, statusCode, body) in attempts)
                {
                    sb.AppendLine($"=== {label} ===");
                    sb.AppendLine($"Status: {statusCode}");
                    sb.AppendLine($"Body: {body}");
                    sb.AppendLine();
                }

                return Content(sb.ToString(), "text/plain");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }



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
                    message = "TSM entry processed successfully.",
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

        // =====================================================
        // TSM ENTRY UPDATE (strict) — PUT api/BgEmployee/TsmEntry/{tsmCode}
        // =====================================================

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

        // =====================================================
        // GET ALL
        // =====================================================

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.Get();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetById(id);

                if (result == null)
                    return NotFound(new { message = $"Employee with ID {id} not found." });

                var mappings = await _service.GetRoleMappings(id);

                var response = new
                {
                    result.Id,
                    result.EmployeeCode,
                    result.TsmCode,
                    result.AreaOfficeId,
                    result.FirstName,
                    result.LastName,
                    result.Gender,
                    result.Mobile,
                    result.State,
                    result.City,
                    result.Pincode,
                    result.DateOfBirth,
                    result.DateOfJoin,
                    result.EffectiveDate,
                    result.ReportingTo,
                    result.IsActive,
                    result.Department,
                    result.ProfileId,
                    result.EmailId,
                    result.Email,
                    result.Password,
                    result.MappedZones,
                    result.MappedZoneIds,
                    result.ProfileImage,
                    result.DealerCode,
                    result.LocationCode,
                    result.CreatedBy,
                    result.CreatedDate,
                    result.UpdatedBy,
                    result.UpdatedDate,

                    selectedDepartments = mappings.Select(m => m.Category).Distinct().ToList(),
                    roles = mappings.Select(m => m.RoleName).Distinct().ToList(),
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // SAVE (INSERT)
        // =====================================================

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] BgEmployeeViewModel model)
        {
            try
            {
                var result = await _service.Create(model);

                var response = new
                {
                    result.Id,
                    result.EmployeeCode,
                    result.TsmCode,
                    result.AreaOfficeId,
                    result.FirstName,
                    result.LastName,
                    result.Gender,
                    result.Mobile,
                    result.State,
                    result.City,
                    result.Pincode,
                    result.DateOfBirth,
                    result.DateOfJoin,
                    result.EffectiveDate,
                    result.ReportingTo,
                    result.IsActive,
                    result.Department,
                    result.ProfileId,
                    result.EmailId,
                    result.Email,
                    result.MappedZones,
                    result.MappedZoneIds,
                    result.ProfileImage,
                    result.DealerCode,
                    result.LocationCode,
                    result.CreatedBy,
                    result.CreatedDate,
                    result.UpdatedBy,
                    result.UpdatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // UPDATE
        // =====================================================

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BgEmployeeViewModel model)
        {
            try
            {
                model.Id = id;
                var result = await _service.Update(model);

                if (result == 0)
                    return NotFound(new { message = $"Employee with ID {id} not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // TOGGLE STATUS
        // =====================================================

        public class ToggleStatusDto
        {
            public bool IsActive { get; set; }
        }

        [HttpPatch("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] ToggleStatusDto dto)
        {
            try
            {
                var result = await _service.UpdateStatus(id, dto.IsActive);

                if (result == 0)
                    return NotFound(new { message = $"Employee with ID {id} not found." });

                return Ok(new { message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // DELETE
        // =====================================================

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.Delete(id);

                if (result == 0)
                    return NotFound(new { message = $"Employee with ID {id} not found." });

                return Ok(new { message = "Employee deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("AssignedDealers")]
        public async Task<IActionResult> GetAssignedDealers([FromQuery] int excludeId = 0)
        {
            try
            {
                var result = await _service.GetAssignedDealerCodes(excludeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("ListView")]
        public async Task<IActionResult> GetListView()
        {
            try
            {
                var result = await _service.GetEmployeeListView();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // DOWNLOAD
        // =====================================================

        [HttpGet("Download")]
        public async Task<IActionResult> Download()
        {
            try
            {
                var file = await _service.DownloadBgEmployeeExcel();

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "BgEmployeeList.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}