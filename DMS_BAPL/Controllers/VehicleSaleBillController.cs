using DMS_BAPL_Data.Services.VehicleSaleBillService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleSaleBillController : ControllerBase
    {
        private readonly IVehicleSaleBillService _vehicleSaleBillService;
        public VehicleSaleBillController(IVehicleSaleBillService vehicleSaleBill)
        {
           _vehicleSaleBillService = vehicleSaleBill; 
        }
        [HttpPost]
        public async Task<IActionResult> Create(VehicleSaleBillEditCreateViewModel model)
        {
            var id = await _vehicleSaleBillService.CreateAsync(model);
            return Ok(id);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _vehicleSaleBillService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _vehicleSaleBillService.GetAllAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VehicleSaleBillEditCreateViewModel model)
        {
            await _vehicleSaleBillService.UpdateAsync(id, model);
            return Ok("Updated Successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _vehicleSaleBillService.DeleteAsync(id);
            return Ok("Deleted Successfully");
        }
    }
}
