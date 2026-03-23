using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.VehicleDispatchService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/vehicle-dispatch")]
    [ApiController]
    public class VehicleDispatchController : ControllerBase
    {
        private readonly IVehicleDispatchService _vehicleDispatchService;

        public VehicleDispatchController(IVehicleDispatchService vehicleDispatchService)
        {
            _vehicleDispatchService = vehicleDispatchService;
        }

        [HttpGet("GetByVehicleStatus")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDispatch>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VehicleDispatch>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var vehicleDispatch = await _vehicleDispatchService.Get();

                return Ok(vehicleDispatch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VehicleDispatch>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VehicleDispatch>>> GetVehicleByStatus([FromQuery] bool status)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var vehicleDispatch = await _vehicleDispatchService.GetVehicleByStatus(status);

                return Ok(vehicleDispatch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
