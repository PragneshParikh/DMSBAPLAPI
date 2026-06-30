using DMS_BAPL_Data.Services.VehicleInfoService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleInfoController : ControllerBase
    {
        private readonly IVehicleInfoService _vehicleInfoService;
        public VehicleInfoController(IVehicleInfoService vehicleInfoService)
        {
            _vehicleInfoService = vehicleInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleInfo([FromQuery] string? regNo, [FromQuery] string? chassisNo, [FromQuery] string? dealerCode)
        {
            try
            {

                var result = await _vehicleInfoService.GetVehicleInfoByRegNoChassis(regNo, chassisNo,dealerCode);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message = "Vehicle not found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateVehicleInfo([FromBody] UpdateVehicleInfoViewModel model)
        {
            await _vehicleInfoService.UpdateVehicleInfo(model);

            return Ok(new
            {
                Message = "Vehicle updated successfully"
            });
        }
    }
}

