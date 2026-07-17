using DMS_BAPL_Data.Services.VehicleQuotationService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleQuotationController : ControllerBase
    {
        private readonly IVehicleQuotationService _vehicleQuotationService;
        public VehicleQuotationController(IVehicleQuotationService vehicleQuotationService)
        {
            _vehicleQuotationService = vehicleQuotationService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _vehicleQuotationService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var result = await _vehicleQuotationService.GetByIdAsync(id);
                if (result == null)
                    return NotFound($"Vehicle quotation with id {id} was not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("generate-quotation-no")]
        public async Task<IActionResult> GenerateQuotationNo()
        {
            try
            {
                var result = await _vehicleQuotationService.GenerateQuotationNo();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("Save")]
        public async Task<IActionResult> SaveQuotation([FromBody] AddVehicleQuotationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var id = await _vehicleQuotationService.SaveAsync(model);
            return Ok(id);
        }
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> updateQuotation(long id, [FromBody] AddVehicleQuotationViewModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Request body cannot be empty.");
                model.Id = id;

                Console.WriteLine($"Route Id : {id}");
                Console.WriteLine($"Model Id : {model.Id}");
                model.Id = id;
                var updated = await _vehicleQuotationService.UpdateAsync(model);
                if (!updated)
                    return NotFound($"Vehicle quotation with id {id} was not found.");
                return Ok(new { Success = true, Message = "Vehicle quotation updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> deleteQuotation(long id)
        {
            try
            {
                var deleted = await _vehicleQuotationService.DeleteAsync(id);
                if (!deleted)
                    return NotFound($"Vehicle quotation with id {id} was not found.");
                return Ok(new { Success = true, Message = "Vehicle quotation deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}