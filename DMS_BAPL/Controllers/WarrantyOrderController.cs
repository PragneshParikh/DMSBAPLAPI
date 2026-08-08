using DMS_BAPL_Data.Services.WarrantyOrderService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyOrderController : ControllerBase
    {
        private readonly IWarrantyOrderService _warrantyOrderService;
        private readonly ILogger<WarrantyOrderController> _logger;

        public WarrantyOrderController(IWarrantyOrderService warrantyOrderService, ILogger<WarrantyOrderController> logger)
        {
            _warrantyOrderService = warrantyOrderService;
            _logger = logger;
        }

        [HttpPost("InsertWarrantyOrder")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertWarrantyOrder(WarrantyOrderViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _warrantyOrderService.CreateWarrantyOrder(model, userId);

                if (result > 0)
                    return Ok(new { message = "Warranty Order saved successfully.", orderId = result });

                _logger.LogError("Failed to insert Warranty Order.");
                return StatusCode(500, "An error occurred while saving the Warranty Order.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                // Unwraps EF's wrapper to the real SQL error (missing table,
                // FK violation, etc.) instead of a generic message.
                var detail = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Database error in InsertWarrantyOrder: {Detail}", detail);
                return StatusCode(500, $"Database error while saving the Warranty Order: {detail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertWarrantyOrder");
                return StatusCode(500, $"An error occurred while saving the Warranty Order: {ex.Message}");
            }
        }

        [HttpPut("UpdateWarrantyOrder")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWarrantyOrder(WarrantyOrderViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var updated = await _warrantyOrderService.UpdateWarrantyOrder(model, userId);

                if (!updated)
                    return NotFound($"Warranty Order with Id {model.Id} not found.");

                return Ok(new { message = "Warranty Order updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Database error in UpdateWarrantyOrder: {Detail}", detail);
                return StatusCode(500, $"Database error while updating the Warranty Order: {detail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateWarrantyOrder");
                return StatusCode(500, $"An error occurred while updating the Warranty Order: {ex.Message}");
            }
        }

        [HttpDelete("DeleteWarrantyOrder/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWarrantyOrder(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var deleted = await _warrantyOrderService.DeleteWarrantyOrder(id, userId);

                if (!deleted)
                    return NotFound($"Warranty Order with Id {id} not found.");

                return Ok(new { message = "Warranty Order deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteWarrantyOrder");
                return StatusCode(500, "An error occurred while deleting the Warranty Order.");
            }
        }

        [HttpGet("GetWarrantyOrderById/{id}")]
        [ProducesResponseType(typeof(WarrantyOrderViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarrantyOrderById(int id)
        {
            try
            {
                var result = await _warrantyOrderService.GetWarrantyOrderById(id);

                if (result == null)
                    return NotFound($"Warranty Order with Id {id} not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWarrantyOrderById");
                return StatusCode(500, "An error occurred while fetching the Warranty Order.");
            }
        }

        [HttpPost("SearchWarrantyOrders")]
        [ProducesResponseType(typeof(WarrantyOrderSearchResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchWarrantyOrders(WarrantyOrderSearchViewModel filter)
        {
            try
            {
                var result = await _warrantyOrderService.SearchWarrantyOrders(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchWarrantyOrders");
                return StatusCode(500, "An error occurred while searching Warranty Orders.");
            }
        }

        [HttpGet("GetNextOrderNumbers")]
        [ProducesResponseType(typeof(NextOrderNumberViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNextOrderNumbers([FromQuery] string dealerCode)
        {
            try
            {
                var result = await _warrantyOrderService.GetNextOrderNumbers(dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNextOrderNumbers");
                return StatusCode(500, "An error occurred while generating the next order numbers.");
            }
        }

        [HttpGet("GetWarrantyJCClaimById/{id}")]
        [ProducesResponseType(typeof(WarrantyJCClaimFullViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarrantyJCClaimById(int id)
        {
            try
            {
                var result = await _warrantyOrderService.GetWarrantyJCClaimById(id);

                if (result == null)
                    return NotFound($"Warranty Claim with Id {id} not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWarrantyJCClaimById");
                return StatusCode(500, "An error occurred while fetching the Warranty Claim.");
            }
        }

        [HttpGet("PrintWarrantyOrder/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PrintWarrantyOrder(int id)
        {
            try
            {
                var pdfBytes = await _warrantyOrderService.GenerateWarrantyOrderPdf(id);
                return File(pdfBytes, "application/pdf", $"WarrantyOrder_{id}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PrintWarrantyOrder");
                return StatusCode(500, $"An error occurred while generating the Warranty Order PDF: {ex.Message}");
            }
        }
    }
}