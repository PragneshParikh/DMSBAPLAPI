using DMS_BAPL_Data.Repositories.WarrantyInvoiceRepo;
using DMS_BAPL_Data.Services.ErpIntegration;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarrantyInvoiceController : ControllerBase
    {
        private readonly IWarrantyInvoiceRepo _warrantyInvoiceRepo;
        private readonly ILogger<WarrantyInvoiceController> _logger;
        private readonly IErpIntegrationService _erpIntegrationService;
        private readonly IConfiguration _configuration;

        public WarrantyInvoiceController(IWarrantyInvoiceRepo warrantyInvoiceRepo,
            ILogger<WarrantyInvoiceController> logger,
            IErpIntegrationService erpIntegrationService,
            IConfiguration configuration)
        {
            _warrantyInvoiceRepo = warrantyInvoiceRepo;
            _logger = logger;
            _erpIntegrationService = erpIntegrationService;
            _configuration = configuration;
        }

        // Adjust this to however the rest of the app resolves the current
        // user id (e.g. from claims/JWT) - left as a placeholder mirroring
        // the pattern likely already used in WarrantyOrderController.
        private string CurrentUserId => User?.Identity?.Name ?? "system";

        // Kill-switch for the auto-send-on-save behavior below, without
        // needing a redeploy if the ERP integration ever needs to be
        // paused (e.g. during an ERP-side outage or a bad payload causing
        // repeated failures). Defaults to on.
        private bool AutoSubmitToErpEnabled => _configuration.GetValue("ErpIntegration:AutoSubmitOnSave", true);

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

                // Auto-push to ERP right after the save commits. This is
                // best-effort: the invoice is already saved by this point,
                // so a failed/unreachable ERP call must not turn a
                // successful save into a 500 - the failure is only
                // surfaced in the response. Retrying means building the
                // payload again (e.g. re-fetch via GetWarrantyInvoiceById
                // and re-run BuildErpWarrantyClaimPayload on the caller's
                // side, or re-save) and posting it to SendErpPayload.
                var erpResult = await TrySubmitToErpAfterSave(invoiceId);

                return Ok(new
                {
                    message = "Warranty Invoice saved successfully.",
                    invoiceId,
                    erp = erpResult
                });
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

                // Same best-effort auto-push as InsertWarrantyInvoice - an
                // update usually means the line data changed, so ERP gets
                // the latest snapshot without a separate manual step.
                var erpResult = await TrySubmitToErpAfterSave(model.Id);

                return Ok(new
                {
                    message = "Warranty Invoice updated successfully.",
                    erp = erpResult
                });
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

        [HttpGet("SearchInvoiceBatchNos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchInvoiceBatchNos([FromQuery] string dealerCode, [FromQuery] string searchText)
        {
            try
            {
                var result = await _warrantyInvoiceRepo.SearchInvoiceBatchNos(dealerCode, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchInvoiceBatchNos");
                return StatusCode(500, $"An error occurred while searching batch numbers: {ex.Message}");
            }
        }

        [HttpGet("SearchInvoiceNos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchInvoiceNos([FromQuery] string dealerCode, [FromQuery] string searchText)
        {
            try
            {
                var result = await _warrantyInvoiceRepo.SearchInvoiceNos(dealerCode, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchInvoiceNos");
                return StatusCode(500, $"An error occurred while searching invoice numbers: {ex.Message}");
            }
        }

        [HttpGet("GetDistinctInvoiceLocations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDistinctInvoiceLocations([FromQuery] string dealerCode)
        {
            try
            {
                var result = await _warrantyInvoiceRepo.GetDistinctInvoiceLocations(dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDistinctInvoiceLocations");
                return StatusCode(500, $"An error occurred while loading invoice locations: {ex.Message}");
            }
        }

        [HttpGet("SearchClaimInvoiceNos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchClaimInvoiceNos([FromQuery] string dealerCode, [FromQuery] string searchText)
        {
            try
            {
                var result = await _warrantyInvoiceRepo.SearchClaimInvoiceNos(dealerCode, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchClaimInvoiceNos");
                return StatusCode(500, $"An error occurred while searching claim invoice numbers: {ex.Message}");
            }
        }

        [HttpGet("GenerateWarrantyInvoicePartPdf/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateWarrantyInvoicePartPdf(int id)
        {
            try
            {
                var pdfBytes = await _warrantyInvoiceRepo.GenerateWarrantyInvoicePartPdf(id);
                return File(pdfBytes, "application/pdf", $"WarrantyInvoicePart_{id}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateWarrantyInvoicePartPdf");
                return StatusCode(500, $"An error occurred while generating the Part Invoice PDF: {ex.Message}");
            }
        }

        [HttpGet("GenerateWarrantyInvoiceLabourPdf/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateWarrantyInvoiceLabourPdf(int id)
        {
            try
            {
                var pdfBytes = await _warrantyInvoiceRepo.GenerateWarrantyInvoiceLabourPdf(id);
                return File(pdfBytes, "application/pdf", $"WarrantyInvoiceLabour_{id}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateWarrantyInvoiceLabourPdf");
                return StatusCode(500, $"An error occurred while generating the Labour Invoice PDF: {ex.Message}");
            }
        }

        [HttpGet("GenerateWarrantyClaimTagPdf/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateWarrantyClaimTagPdf(int id)
        {
            try
            {
                var pdfBytes = await _warrantyInvoiceRepo.GenerateWarrantyClaimTagPdf(id);
                return File(pdfBytes, "application/pdf", $"WarrantyClaimTag_{id}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateWarrantyClaimTagPdf");
                return StatusCode(500, $"An error occurred while generating the Warranty Claim Tag PDF: {ex.Message}");
            }
        }

        [HttpPost("SendErpPayload")]
        [ProducesResponseType(typeof(ErpSubmitResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendErpPayload([FromBody] ErpWarrantyClaimSubmitRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request?.Value == null || request.Value.Count == 0)
                return BadRequest("At least one claim line (Value) is required.");

            try
            {
                var result = await _erpIntegrationService.SubmitWarrantyClaimLines(request);

                if (!result.Success)
                    return StatusCode(500, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendErpPayload");
                return StatusCode(500, $"An error occurred while sending the payload to ERP: {ex.Message}");
            }
        }

        private async Task<ErpSubmitResult> BuildAndSubmitErpPayload(int invoiceId)
        {
            var lines = await _warrantyInvoiceRepo.BuildErpWarrantyClaimPayload(invoiceId);

            var request = new ErpWarrantyClaimSubmitRequest
            {
                VendorId = _configuration.GetValue<int>("ErpIntegration:VendorId"),
                SubVendorCode = _configuration["ErpIntegration:SubVendorCode"],
                Value = lines
            };

            return await _erpIntegrationService.SubmitWarrantyClaimLines(request);
        }
        private async Task<ErpSubmitResult?> TrySubmitToErpAfterSave(int invoiceId)
        {
            if (!AutoSubmitToErpEnabled)
            {
                _logger.LogInformation(
                    "Skipping auto ERP submit for Warranty Invoice {InvoiceId} - ErpIntegration:AutoSubmitOnSave is disabled.",
                    invoiceId);
                return null;
            }

            try
            {
                return await BuildAndSubmitErpPayload(invoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-submit to ERP failed for saved Warranty Invoice {InvoiceId}", invoiceId);
                return new ErpSubmitResult
                {
                    Success = false,
                    Message = $"Warranty Invoice saved, but sending it to ERP failed: {ex.Message}",
                    LinesSent = 0
                };
            }
        }
    }

}