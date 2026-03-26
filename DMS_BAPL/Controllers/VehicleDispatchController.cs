using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.VehicleDispatchService;
using DMS_BAPL_Utils.Helpers;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Inserts a list of vehicle dispatch records into the system.
        /// Validates the input, calls the service layer to save the data,
        /// and returns appropriate HTTP responses based on the outcome.
        /// </summary>
        /// <param name="vehicleDispatch">List of vehicle dispatch details to be inserted.</param>
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
        public async Task<IActionResult> InsertVehicleDispatchDetails([FromBody] List<VehicleDispatch> vehicleDispatch)
        {
            try
            {
                if (vehicleDispatch is null)
                    return BadRequest(new { message = "Invalid data" });

                await _vehicleDispatchService.InsertVehicleDispatchDetail(vehicleDispatch);

                return Ok("Data inserted sucessfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while inserting vehicle dispatch details.");
                throw;
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

                var vehicleDispatch = await _vehicleDispatchService.UpdateInvoiceStatus(invoiceNo, userId);

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
