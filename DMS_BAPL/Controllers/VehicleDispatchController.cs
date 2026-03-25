using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.VehicleDispatchService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/vehicle-dispatch")]
    [ApiController]
    public class VehicleDispatchController : ControllerBase
    {
        private readonly IVehicleDispatchService _vehicleDispatchService;
        private readonly ILogger<VehicleDispatchController> _logger;

        public VehicleDispatchController(IVehicleDispatchService vehicleDispatchService, ILogger<VehicleDispatchController> logger)
        {
            _vehicleDispatchService = vehicleDispatchService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all vehicle dispatch records.
        /// </summary>
        /// <returns>A list of vehicle dispatch records.</returns>
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
                _logger.LogError("Error occurred while fetching vehicle dispatch records");
                throw;
            }
        }

        /// <summary>
        /// Retrieves vehicle dispatch records filtered by acceptance status and dealer code.
        /// </summary>
        /// <param name="status">Indicates whether the vehicle dispatch is accepted (true) or not (false).</param>
        /// <param name="dealerCode">The unique code identifying the dealer.</param>
        /// <returns>A list of vehicle dispatch records matching the specified status and dealer code.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VehicleDispatch>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VehicleDispatch>>> GetVehicleByStatus([FromQuery] bool status, string dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var vehicleDispatch = await _vehicleDispatchService.GetVehicleByStatus(dealerCode, status);

                return Ok(vehicleDispatch);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching vehicle by status");
                throw;
            }
        }
    }
}
