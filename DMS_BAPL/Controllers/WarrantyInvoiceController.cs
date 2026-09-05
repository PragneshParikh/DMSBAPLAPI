using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.WarrantyInvoiceRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarrantyInvoiceController : ControllerBase
    {
        private readonly IWarrantyInvoiceRepo _warrantyInvoiceRepo;
        private readonly ILogger<WarrantyInvoiceController> _logger;
        private readonly IConfiguration _configuration;
        private readonly BapldmsvadContext _context;

        public WarrantyInvoiceController(IWarrantyInvoiceRepo warrantyInvoiceRepo,
            ILogger<WarrantyInvoiceController> logger,
            IConfiguration configuration,
            BapldmsvadContext context)
        {
            _warrantyInvoiceRepo = warrantyInvoiceRepo;
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

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

                return Ok(new
                {
                    message = "Warranty Invoice saved successfully.",
                    invoiceId
                });
            }
            catch (Exception ex)
            {
                var root = ex;
                while (root.InnerException != null)
                    root = root.InnerException;

                _logger.LogError(ex, "Error in InsertWarrantyInvoice. Root cause: {RootMessage}", root.Message);
                return StatusCode(500, $"An error occurred while saving the Warranty Invoice: {root.Message}");
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

                return Ok(new
                {
                    message = "Warranty Invoice updated successfully."
                });
            }
            catch (Exception ex)
            {
                var root = ex;
                while (root.InnerException != null)
                    root = root.InnerException;

                _logger.LogError(ex, "Error in UpdateWarrantyInvoice. Root cause: {RootMessage}", root.Message);
                return StatusCode(500, $"An error occurred while updating the Warranty Invoice: {root.Message}");
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

        // ===================================================================
        // ERP submission - one raw ErpWarrantyClaimLineViewModel object per
        // POST (no array, no wrapper - confirmed against the ERP's own
        // rejection messages). Each line retries with a fresh UniqueId if
        // the ERP reports that one as already taken.
        // ===================================================================

        [HttpPost("UATWarrantyData")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendToERP([FromBody] SendWarrantyInvoiceToErpRequest request)
        {
            string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authorized");

            if (request == null || request.InvoiceId <= 0)
                return BadRequest("A valid invoiceId is required.");

            try
            {
                var lines = await BuildErpPayload(request.InvoiceId);
                if (lines.Count == 0)
                    return BadRequest($"Warranty Invoice {request.InvoiceId} has no claim lines to send to ERP.");

                var responses = new List<string>();
                foreach (var line in lines)
                    responses.Add(await PostToErpAsync(line));

                return Ok(new
                {
                    invoiceLineResponses = responses
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Warranty Invoice {InvoiceId} to ERP", request.InvoiceId);
                return StatusCode(500, $"An error occurred while sending Warranty Invoice {request.InvoiceId} to ERP: {ex.Message}");
            }
        }

        private async Task<List<ErpWarrantyClaimLineViewModel>> BuildErpPayload(int invoiceId)
        {
            var header = await _context.WarrantyInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId)
                ?? throw new InvalidOperationException($"Warranty Invoice with Id {invoiceId} not found.");

            var dealer = !string.IsNullOrWhiteSpace(header.DealerCode)
                ? await _context.DealerMasters.FirstOrDefaultAsync(d => d.Dealercode == header.DealerCode)
                : null;


            PurchaseOrder? latestDealerPo = !string.IsNullOrWhiteSpace(header.DealerCode)
                ? await _context.PurchaseOrders
                    .Where(p => p.CustomerCode == header.DealerCode && p.ErpPoNumber != null)
                    .OrderByDescending(p => p.ErpSubmittedDate)
                    .ThenByDescending(p => p.Id)
                    .FirstOrDefaultAsync()
                : null;

            string vendorPoNo = latestDealerPo?.ErpPoNumber ?? "";
            string vendorPoDate = latestDealerPo?.ErpPoDate?.ToString("dd-MM-yyyy") ?? "";

            var orderIds = await _context.WarrantyInvoiceDetails
                .Where(d => d.WarrantyInvoiceHeaderId == invoiceId)
                .Select(d => d.WarrantyOrderHeaderId)
                .ToListAsync();

            // Only "Part" lines - Labour is excluded from what's sent to the ERP.
            var gridRows = await _context.WarrantyOrderGridDetails
                .Where(g => orderIds.Contains(g.WarrantyOrderHeaderId) && g.ItemType == "Part")
                .ToListAsync();

            var lines = new List<ErpWarrantyClaimLineViewModel>();
            int srNo = 1;

            foreach (var g in gridRows)
            {
                var chassisDetail = !string.IsNullOrWhiteSpace(g.ChassisNo)
                    ? await _context.ChassisDetails.FirstOrDefaultAsync(c => c.ChassisNo == g.ChassisNo)
                    : null;

                string? modelName = null;
                if (!string.IsNullOrWhiteSpace(chassisDetail?.ItemCode))
                {
                    var item = await _context.ItemMasters.FirstOrDefaultAsync(i => i.Itemcode == chassisDetail.ItemCode);
                    modelName = item?.Itemname ?? item?.Displayname;
                }

                var claimFfirId = await _context.WarrantyJcclaims
                    .Where(c => c.Id == g.WarrantyJcclaimId)
                    .Select(c => c.Ffirid)
                    .FirstOrDefaultAsync();

                DateTime? failureDate = claimFfirId.HasValue
                    ? await _context.Ffirheaders.Where(f => f.Id == claimFfirId.Value).Select(f => f.FailureDate).FirstOrDefaultAsync()
                    : null;

                var claimDetail = await _context.WarrantyJcclaimDetails
                    .Where(d => d.WarrantyJcclaimHeaderId == g.WarrantyJcclaimId && d.ItemType == g.ItemType)
                    .FirstOrDefaultAsync();

                decimal totalTax = (g.CgstAmount ?? 0) + (g.SgstAmount ?? 0) + (g.IgstAmount ?? 0);
                decimal qty = g.Quantity ?? 0;
                decimal rate = qty > 0 ? Math.Round(((g.TotalAmount ?? 0) - totalTax) / qty, 2) : 0;

                lines.Add(new ErpWarrantyClaimLineViewModel
                {
                    SlNo = srNo++,
                    DealerName = dealer?.Compname ?? "",
                    DealerCode = header.DealerCode ?? "",
                    Location = g.LocationName ?? "",
                    LocationCity = "",
                    JobNo = Truncate(g.JobCardNo, 20),
                    JobDate = g.JobCardDate?.ToString("dd-MM-yyyy") ?? "",
                    ClaimNo = g.ClaimNo ?? "",
                    ClaimDate = g.ClaimDate?.ToString("dd-MM-yyyy") ?? "",
                    Kms = ((int?)g.Kms)?.ToString() ?? "",
                    VehicleSaleDate = chassisDetail?.SaleDate?.ToString("dd-MM-yyyy") ?? "",
                    PartFailureDate = failureDate?.ToString("dd-MM-yyyy") ?? "",
                    ServiceType = g.ServiceHead ?? "",
                    ChassisNo = g.ChassisNo ?? "",
                    ModelName = modelName ?? "",
                    Variants = "",
                    PartCode = g.PartCode ?? "",
                    PartName = g.PartName ?? "",
                    Qty = qty,
                    Rate = rate,
                    CgstPercent = (g.CgstPercent ?? 0).ToString("0.##"),
                    CgstAmount = g.CgstAmount ?? 0,
                    SgstPercent = (g.SgstPercent ?? 0).ToString("0.##"),
                    SgstAmount = g.SgstAmount ?? 0,
                    IgstPercent = (g.IgstPercent ?? 0).ToString("0.##"),
                    IgstAmount = g.IgstAmount ?? 0,
                    Amount = g.TotalAmount ?? 0,
                    DealerObservation = claimDetail?.DealerObservation ?? "",
                    Rca = claimDetail?.RootCauseAnalysis ?? "",
                    InvoiceRefNo = "",
                    InvoiceNo = g.InvoiceNo ?? "",
                    InvoiceDate = g.InvoiceDate?.ToString("dd-MM-yyyy") ?? "",
                    DocNo = $"{header.InvoicePrefix}{header.InvoiceNo}",
                    DocDate = header.InvoiceDate?.ToString("dd-MM-yyyy") ?? "",
                    VendorPoNo = vendorPoNo,
                    VendorPoDate = vendorPoDate,
                    PoNo = "",
                    PoDate = "",
                    Total = g.TotalAmount ?? 0,
                    UniqueId = await GetNextErpUniqueIdAsync(),
                });
            }

            return lines;
        }

        private static string Truncate(string? value, int maxLen) =>
            string.IsNullOrEmpty(value) || value.Length <= maxLen ? value ?? "" : value[..maxLen];
        private async Task<int> GetNextErpUniqueIdAsync()
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT NEXT VALUE FOR dbo.ErpUniqueIdSequence";
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

 
        private class ErpLineResponse
        {
            public bool Succeed { get; set; }
            public string? UniqueId { get; set; }
            public string? ConfirmMessage { get; set; }

            [JsonPropertyName("PO No")]
            public string? PoNo { get; set; }

            [JsonPropertyName("PO Date")]
            public string? PoDate { get; set; }
        }

        private async Task<string> PostToErpAsync(ErpWarrantyClaimLineViewModel line)
        {
            var baseUrl = _configuration["ErpIntegration:BaseUrl"]
                ?? "https://uatbaplai-cpapc4h7gvdkfxh4.centralindia-01.azurewebsites.net";
            var path = _configuration["ErpIntegration:WarrantyDataPath"] ?? "/api/UATWarrantyData";
            var requestUrl = $"{baseUrl.TrimEnd('/')}{path}";

            const int maxAttempts = 5;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                using var client = new HttpClient();
                var json = JsonSerializer.Serialize(line);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

   
                await LogApiTrackingAsync("WarrantyInvoice/UATWarrantyData", json, $"{(int)response.StatusCode} (attempt {attempt})", responseBody);

                response.EnsureSuccessStatusCode();

                ErpLineResponse? parsed;
                try
                {
                    parsed = JsonSerializer.Deserialize<ErpLineResponse>(responseBody);
                }
                catch (JsonException)
                {
                    return responseBody;
                }

                bool isDuplicateId = parsed != null && !parsed.Succeed &&
                    (parsed.ConfirmMessage?.Contains("already exist", StringComparison.OrdinalIgnoreCase) ?? false);

                if (!isDuplicateId || attempt == maxAttempts)
                    return responseBody;

                line.UniqueId = await GetNextErpUniqueIdAsync();
            }

            throw new InvalidOperationException("Failed to submit line to ERP after retrying with new UniqueIds.");
        }

        private async Task LogApiTrackingAsync(string endpoint, string? payload, string? status, string? response)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO APITracking (endpoint, dateofhit, payload, status, response)
            VALUES ({endpoint}, {DateTime.Now}, {payload}, {status}, {response})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write APITracking row");
            }
        }
    }
}