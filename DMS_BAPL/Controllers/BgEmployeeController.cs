using DMS_BAPL_Data.Services.BgEmployeeMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
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

                return Ok(result);
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
                return Ok(result);
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
    }
}