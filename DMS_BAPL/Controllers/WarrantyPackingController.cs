using DMS_BAPL_Data.Repositories.WarrantyPackingRepo;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyPackingController : ControllerBase
    {
        private readonly IWarrantyPackingRepo _warrantyPackingRepo;
        private readonly IPrefixService _prefixService;
        private readonly ILogger<WarrantyPackingController> _logger;

        public WarrantyPackingController(
            IWarrantyPackingRepo warrantyPackingRepo,
            IPrefixService prefixService,
            ILogger<WarrantyPackingController> logger)
        {
            _warrantyPackingRepo = warrantyPackingRepo;
            _prefixService = prefixService;
            _logger = logger;
        }

        [HttpGet("GetPackableLines/{warrantyInvoiceHeaderId}")]
        [ProducesResponseType(typeof(List<PackingSlipLineViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackableLines(int warrantyInvoiceHeaderId)
        {
            try
            {
                var lines = await _warrantyPackingRepo.GetPackableLines(warrantyInvoiceHeaderId);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching packable lines for WarrantyInvoiceHeaderId: {WarrantyInvoiceHeaderId}", warrantyInvoiceHeaderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost("InsertWarrantyPackingSlip")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertWarrantyPackingSlip([FromBody] WarrantyPackingSlipViewModel model)
        {
            if (model == null)
                return BadRequest("Model cannot be null.");

            if (model.Boxes == null || model.Boxes.Count == 0)
                return BadRequest("At least one box with packed lines is required.");

            try
            {
                var userId = User?.Identity?.Name ?? "system";

                // Advance the prefix BEFORE saving - if the module's config
                // row is missing, fail cleanly with nothing persisted rather
                // than silently succeeding and reporting failure.
                await _prefixService.UpdateNextNumberByDealerByModule(model.DealerCode, "wpack_prefix");

                var result = await _warrantyPackingRepo.InsertWarrantyPackingSlip(model, userId);

                return Ok(new { message = "Warranty Packing Slip saved successfully.", packingSlipId = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while inserting warranty packing slip.");
                return Unauthorized("You are not authorized to perform this action.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting warranty packing slip.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost("SearchWarrantyPackingSlips")]
        [ProducesResponseType(typeof(WarrantyPackingSlipSearchResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchWarrantyPackingSlips([FromBody] WarrantyPackingSlipSearchViewModel filter)
        {
            try
            {
                var result = await _warrantyPackingRepo.SearchWarrantyPackingSlips(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching warranty packing slips.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GetWarrantyPackingSlipById/{id}")]
        [ProducesResponseType(typeof(WarrantyPackingSlipDetailsViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarrantyPackingSlipById(int id)
        {
            try
            {
                var result = await _warrantyPackingRepo.GetWarrantyPackingSlipById(id);
                if (result == null)
                    return NotFound($"Warranty Packing Slip with Id {id} not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching warranty packing slip {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("DeleteWarrantyPackingSlip/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWarrantyPackingSlip(int id)
        {
            try
            {
                var userId = User?.Identity?.Name ?? "system";
                var success = await _warrantyPackingRepo.DeleteWarrantyPackingSlip(id, userId);

                if (!success)
                    return NotFound($"Warranty Packing Slip with Id {id} not found or already deleted.");

                return Ok(new { message = "Warranty Packing Slip deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting warranty packing slip {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost("SearchWarrantyPackingSlipLines")]
        [ProducesResponseType(typeof(WarrantyPackingSlipLineSearchResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchWarrantyPackingSlipLines([FromBody] WarrantyPackingSlipLineSearchViewModel filter)
        {
            try
            {
                var result = await _warrantyPackingRepo.SearchWarrantyPackingSlipLines(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching warranty packing slip lines.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("SearchPackingSlipNos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchPackingSlipNos([FromQuery] string? dealerCode, [FromQuery] string searchText)
        {
            try
            {
                var result = await _warrantyPackingRepo.SearchPackingSlipNos(dealerCode, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching packing slip numbers.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("SearchPackingInvoiceNos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchPackingInvoiceNos([FromQuery] string? dealerCode, [FromQuery] string searchText)
        {
            try
            {
                var result = await _warrantyPackingRepo.SearchPackingInvoiceNos(dealerCode, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching packing invoice numbers.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GenerateWarrantyPackingSlipPdf/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateWarrantyPackingSlipPdf(int id)
        {
            try
            {
                var pdfBytes = await _warrantyPackingRepo.GenerateWarrantyPackingSlipPdf(id);
                return File(pdfBytes, "application/pdf", $"WarrantyPackingSlip_{id}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating warranty packing slip PDF {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while generating the Warranty Packing Slip PDF: {ex.Message}");
            }
        }


    }
}