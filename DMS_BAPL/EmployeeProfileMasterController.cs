using DMS_BAPL_Service.Services.EmployeeProfileMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DMS_BAPL_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeProfileMasterController : ControllerBase
    {
        private readonly IEmployeeProfileMasterService _service;

        public EmployeeProfileMasterController(IEmployeeProfileMasterService service)
        {
            _service = service;
        }

        // =====================================================
        // GET ALL PROFILES (dropdown data — read-only)
        // GET api/EmployeeProfileMaster/GetAll
        // Returns profiles ordered by SortOrder:
        //   City Head(1) → District Head(2) → Zone Head(3)
        //   → State Head(4) → National Head(5)
        // =====================================================

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllProfiles();

                // Map to ViewModel — explicit shape so Angular
                // receives { id, profileName, sortOrder } consistently
                var vm = result.Select(p => new EmployeeProfileMasterViewModel
                {
                    Id = p.Id,
                    ProfileName = p.ProfileName,
                    SortOrder = p.SortOrder,
                });

                return Ok(vm);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // GET MAPPINGS FOR A BG EMPLOYEE
        // GET api/EmployeeProfileMaster/GetMappings/{bgEmployeeId}
        // =====================================================

        [HttpGet("GetMappings/{bgEmployeeId}")]
        public async Task<IActionResult> GetMappings(int bgEmployeeId)
        {
            try
            {
                var result = await _service.GetMappingsByBgEmployee(bgEmployeeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =====================================================
        // SAVE MAPPINGS (replaces all for the BG employee)
        // POST api/EmployeeProfileMaster/SaveMappings
        // =====================================================

        [HttpPost("SaveMappings")]
        public async Task<IActionResult> SaveMappings([FromBody] BgEmployeeProfileMappingSaveRequest request)
        {
            try
            {
                var result = await _service.SaveMappings(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}