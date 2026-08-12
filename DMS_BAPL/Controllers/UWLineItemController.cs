using DMS_BAPL_Data.Repositories.UwLineItemRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/UwLineItem")]
    [ApiController]
    public class UwLineItemController : ControllerBase
    {
        private readonly IUwLineItemRepo _uwLineItemRepo;
        private readonly ILogger<UwLineItemController> _logger;

        public UwLineItemController(IUwLineItemRepo uwLineItemRepo, ILogger<UwLineItemController> logger)
        {
            _uwLineItemRepo = uwLineItemRepo;
            _logger = logger;
        }

        // CurrentUserId placeholder - matches the same placeholder pattern
        // already used in WarrantyInvoiceController, since this app's real
        // user-resolution mechanism wasn't available. Adjust to the app's
        // actual approach.
        private string CurrentUserId => User?.Identity?.Name ?? "system";

        [HttpPost("SearchUwLineItems")]
        [ProducesResponseType(typeof(UwLineItemSearchResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchUwLineItems([FromBody] UwLineItemSearchViewModel filter)
        {
            try
            {
                var result = await _uwLineItemRepo.SearchUwLineItems(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchUwLineItems");
                return StatusCode(500, $"An error occurred while searching UW Line Items: {ex.Message}");
            }
        }

        [HttpPut("ApproveUwLineItem")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveUwLineItem([FromBody] UwLineItemActionViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var (success, errorMessage) = await _uwLineItemRepo.ApproveUwLineItem(model, CurrentUserId);
                if (!success)
                    return NotFound(errorMessage);

                return Ok(new { message = "Claim approved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApproveUwLineItem");
                return StatusCode(500, $"An error occurred while approving the claim: {ex.Message}");
            }
        }

        [HttpPut("RejectUwLineItem")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectUwLineItem([FromBody] UwLineItemActionViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var (success, errorMessage) = await _uwLineItemRepo.RejectUwLineItem(model, CurrentUserId);
                if (!success)
                    return NotFound(errorMessage);

                return Ok(new { message = "Claim rejected successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RejectUwLineItem");
                return StatusCode(500, $"An error occurred while rejecting the claim: {ex.Message}");
            }
        }

        [HttpDelete("DeleteUwLineItem/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUwLineItem(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid line item Id.");

            try
            {
                var (success, errorMessage) = await _uwLineItemRepo.DeleteUwLineItem(id);
                if (!success)
                    return NotFound(errorMessage);

                return Ok(new { message = "Line item deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteUwLineItem");
                return StatusCode(500, $"An error occurred while deleting the line item: {ex.Message}");
            }
        }

    }
}