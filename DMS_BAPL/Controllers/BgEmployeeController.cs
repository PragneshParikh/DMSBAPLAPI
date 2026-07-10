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
        // TSM ERP LOOKUP — NEW
        // GET api/BgEmployee/TsmLookup/{tsmCode}
        // Proxies the external ERP TSM API so the browser never calls
        // that third-party domain directly.
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

        // ── TEMPORARY DIAGNOSTIC — GET api/BgEmployee/TsmLookupRaw/{tsmCode}
        // Tries several calling conventions against the external API and
        // shows every attempt's raw status + body, so we can see which one
        // (if any) actually works instead of guessing one at a time.
        // Remove this action once the real convention is confirmed.
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

        // =====================================================
        // GET ALL
        // GET api/BgEmployee/GetAll
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
        // GET api/BgEmployee/GetById/{id}
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
        // POST api/BgEmployee/Save
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
        // PUT api/BgEmployee/Update/{id}
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
        // DELETE api/BgEmployee/Delete/{id}
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
        // GET api/BgEmployee/Download
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