using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DMS_BAPL_Data.Repositories.WarrantyInvoiceRepo;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarrantyInvoiceController : ControllerBase
    {
        private readonly IWarrantyInvoiceRepo _warrantyInvoiceRepo;
        private readonly ILogger<WarrantyInvoiceController> _logger;

        public WarrantyInvoiceController(IWarrantyInvoiceRepo warrantyInvoiceRepo, ILogger<WarrantyInvoiceController> logger)
        {
            _warrantyInvoiceRepo = warrantyInvoiceRepo;
            _logger = logger;
        }

        // Adjust this to however the rest of the app resolves the current
        // user id (e.g. from claims/JWT) - left as a placeholder mirroring
        // the pattern likely already used in WarrantyOrderController.
        private string CurrentUserId => User?.Identity?.Name ?? "system";

        [HttpPost("InsertWarrantyInvoice")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertWarrantyInvoice([FromBody] WarrantyInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.WarrantyOrderIds == null || model.WarrantyOrderIds.Count == 0)
                return BadRequest("At least one Warranty Order must be linked to this invoice.");

            try
            {
                var invoiceId = await _warrantyInvoiceRepo.InsertWarrantyInvoice(model, CurrentUserId);
                return Ok(new { message = "Warranty Invoice saved successfully.", invoiceId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertWarrantyInvoice");
                return StatusCode(500, $"An error occurred while saving the Warranty Invoice: {ex.Message}");
            }
        }

        [HttpPut("UpdateWarrantyInvoice")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWarrantyInvoice([FromBody] WarrantyInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.WarrantyOrderIds == null || model.WarrantyOrderIds.Count == 0)
                return BadRequest("At least one Warranty Order must be linked to this invoice.");

            try
            {
                var success = await _warrantyInvoiceRepo.UpdateWarrantyInvoice(model, CurrentUserId);
                if (!success)
                    return NotFound($"Warranty Invoice with Id {model.Id} not found or is not active.");

                return Ok(new { message = "Warranty Invoice updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateWarrantyInvoice");
                return StatusCode(500, $"An error occurred while updating the Warranty Invoice: {ex.Message}");
            }
        }

        [HttpDelete("DeleteWarrantyInvoice/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWarrantyInvoice(int id)
        {
            try
            {
                var success = await _warrantyInvoiceRepo.DeleteWarrantyInvoice(id, CurrentUserId);
                if (!success)
                    return NotFound($"Warranty Invoice with Id {id} not found or already deleted.");

                return Ok(new { message = "Warranty Invoice deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteWarrantyInvoice");
                return StatusCode(500, $"An error occurred while deleting the Warranty Invoice: {ex.Message}");
            }
        }

        [HttpGet("GetWarrantyInvoiceById/{id}")]
        [ProducesResponseType(typeof(WarrantyInvoiceViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarrantyInvoiceById(int id)
        {
            try
            {
                var result = await _warrantyInvoiceRepo.GetWarrantyInvoiceById(id);
                if (result == null)
                    return NotFound($"Warranty Invoice with Id {id} not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWarrantyInvoiceById");
                return StatusCode(500, $"An error occurred while fetching the Warranty Invoice: {ex.Message}");
            }
        }

        [HttpPost("SearchWarrantyInvoices")]
        [ProducesResponseType(typeof(WarrantyInvoiceSearchResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchWarrantyInvoices([FromBody] WarrantyInvoiceSearchViewModel filter)
        {
            try
            {
                var result = await _warrantyInvoiceRepo.SearchWarrantyInvoices(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchWarrantyInvoices");
                return StatusCode(500, $"An error occurred while searching Warranty Invoices: {ex.Message}");
            }
        }

        [HttpGet("GetNextInvoiceNumbers")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNextInvoiceNumbers([FromQuery] string dealerCode)
        {
            try
            {
                var (batchNo, invoicePrefix, invoiceNo) = await _warrantyInvoiceRepo.GetNextInvoiceNumbers(dealerCode);
                return Ok(new { batchNo, invoicePrefix, invoiceNo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNextInvoiceNumbers");
                return StatusCode(500, $"An error occurred while generating invoice numbers: {ex.Message}");
            }
        }
    }
}