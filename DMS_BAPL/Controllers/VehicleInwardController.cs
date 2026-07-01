using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.ChassisBatteryDetailService;
using DMS_BAPL_Data.Services.ChassisDetailsService;
using DMS_BAPL_Data.Services.VehicleDispatchService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/vehicle-inward")]
    [ApiController]
    public class VehicleInwardController : ControllerBase
    {
        private readonly IVehicleInwardService _vehicleInwardService;
        private readonly IChassisDetailService _chassisDetailService;
        private readonly IChassisBatteryDetailService _chassisBatteryDetailService;
        private readonly ILogger<VehicleInwardController> _logger;

        public VehicleInwardController(
            IVehicleInwardService vehicleInwardService,
            IChassisDetailService chassisDetailService,
            IChassisBatteryDetailService chassisBatteryDetailService,
            ILogger<VehicleInwardController> logger)
        {
            _vehicleInwardService = vehicleInwardService;
            _chassisDetailService = chassisDetailService;
            _chassisBatteryDetailService = chassisBatteryDetailService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all vehicle dispatch records.
        /// </summary>
        /// <returns>A list of vehicle dispatch records.</returns>
        [HttpGet("GetByVehicleStatus")]
        [ProducesResponseType(typeof(IEnumerable<VehicleInward>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VehicleInward>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var vehicleInward = await _vehicleInwardService.Get();

                return Ok(vehicleInward);
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
        [ProducesResponseType(typeof(IEnumerable<VehicleInward>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VehicleInward>>> GetVehicleByStatus([FromQuery] bool status, string dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var vehicleInward = await _vehicleInwardService.GetVehicleByStatus(dealerCode, status);

                return Ok(vehicleInward);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching vehicle by status");
                throw;
            }
        }

        /// <summary>
        /// Inserts a list of vehicle dispatch records into the system.
        /// Validates the input, calls the service layer to save the data,
        /// and returns appropriate HTTP responses based on the outcome.
        /// </summary>
        /// <param name="vehicleInward">List of vehicle dispatch details to be inserted.</param>
        /// <returns>
        /// 200 OK if insertion is successful,
        /// 400 Bad Request if input is invalid,
        /// 500 Internal Server Error if an exception occurs.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertVehicleInwardDetails([FromBody] VehicleInwardViewModel vehicleInwardViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (vehicleInwardViewModel is null)
                    return BadRequest(new { message = "Invalid data" });

                dynamic result = await _vehicleInwardService.InsertVehicleInwardDetail(vehicleInwardViewModel);

                var success = (bool?)result.GetType().GetProperty("Success")?.GetValue(result) ?? false;

                if (success)
                {
                    await _chassisDetailService.InsertChassis(vehicleInwardViewModel, userId);
                    await _chassisBatteryDetailService.InsertBatteryDetail(vehicleInwardViewModel, userId);
                }

                if (success)
                {
                    result = new
                    {
                        Valid = true,
                        Description = "Data Saved Successfully",
                        Value = new
                        {
                            Msg = "Data Saved Successfully",
                            StatusCode = "200",
                            ResponseStatus = "true"
                        }
                    };
                }
                else
                {
                    result = new
                    {
                        Valid = false,
                        Description = "Data not saved.",
                        Value = (object)null
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Valid = false,
                    Description = "Data not saved.",
                    Value = (object)null
                });
            }
        }

        /// <summary>
        /// Updates the status of an invoice based on the provided invoice number.
        /// Validates the user from the token, then calls the service layer to perform the update.
        /// </summary>
        /// <param name="invoiceNo">The invoice number whose status needs to be updated.</param>
        /// <returns>
        /// 200 OK if the invoice status is updated successfully, 
        /// 401 Unauthorized if the user is not authenticated,
        /// 500 Internal Server Error if an exception occurs.
        /// </returns>
        [HttpPost("UpdateInvoiceStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateInvoiceStatus([FromBody] string invoiceNo)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var vehicleInward = await _vehicleInwardService.UpdateInvoiceStatus(invoiceNo, userId);

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = "Invoice status updated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while updating invoice status");
                throw;
            }
        }

    }
}
