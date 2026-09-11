using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.WarrantyOrderService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyOrderController : ControllerBase
    {
        private readonly IWarrantyOrderService _warrantyOrderService;
        private readonly ILogger<WarrantyOrderController> _logger;
        private readonly IConfiguration _configuration;
        private readonly BapldmsvadContext _context;

        public WarrantyOrderController(
            IWarrantyOrderService warrantyOrderService,
            ILogger<WarrantyOrderController> logger,
            IConfiguration configuration,
            BapldmsvadContext context)
        {
            _warrantyOrderService = warrantyOrderService;
            _logger = logger;
            _configuration = configuration;
            _context = context;
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

        [HttpGet("SearchBatchNos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchBatchNos([FromQuery] string dealerCode, [FromQuery] string searchText)
        {
            try
            {
                var result = await _warrantyOrderService.SearchBatchNos(dealerCode, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchBatchNos");
                return StatusCode(500, $"An error occurred while searching batch numbers: {ex.Message}");
            }
        }

        [HttpGet("SearchOrderNos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchOrderNos([FromQuery] string dealerCode, [FromQuery] string searchText)
        {
            try
            {
                var result = await _warrantyOrderService.SearchOrderNos(dealerCode, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchOrderNos");
                return StatusCode(500, $"An error occurred while searching order numbers: {ex.Message}");
            }
        }

        [HttpGet("GetDistinctOrderLocations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDistinctOrderLocations([FromQuery] string dealerCode)
        {
            try
            {
                var result = await _warrantyOrderService.GetDistinctOrderLocations(dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDistinctOrderLocations");
                return StatusCode(500, $"An error occurred while loading order locations: {ex.Message}");
            }
        }

        // ===================================================================
        // ERP submission
        // ===================================================================

        [HttpPost("UATWarrantyData")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendToERP( [FromBody] SendWarrantyOrderToErpRequest request)
        {
            string userId =
                GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authorized");

            if (request == null || request.OrderId <= 0)
                return BadRequest("A valid orderId is required.");

            try
            {
                // Get saved Warranty Order + Grid Details
                var (payload, order) =
                    await BuildErpPayload(request.OrderId);

                // No Part rows
                if (payload.PoLine == null ||
                    payload.PoLine.Count == 0)
                {
                    return BadRequest(
                        $"Warranty Order {request.OrderId} has no claim lines to send to ERP.");
                }

                // Send to ERP
                var (responseBody, parsed) =
                    await PostToErpAsync(payload);

                // ERP response could not be parsed
                if (parsed == null)
                {
                    return StatusCode(
                        500,
                        new
                        {
                            orderId = request.OrderId,
                            erpSubmitted = false,
                            message =
                                "ERP returned a response that could not be parsed.",
                            erpResponse = responseBody
                        });
                }

                // ERP rejected request
                if (!parsed.Succeed)
                {
                    return BadRequest(
                        new
                        {
                            orderId = request.OrderId,
                            erpSubmitted = false,
                            message =
                                parsed.ConfirmMessage ??
                                "ERP declined this submission.",
                            erpResponse = responseBody
                        });
                }
                
                if (int.TryParse(
                     payload.PoHeader.RefNo,
                     NumberStyles.Integer,
                     CultureInfo.InvariantCulture,
                     out var finalUniqueId))
                {
                    order.ErpUniqueId = finalUniqueId;
                }

                // Save ERP PO Number
                if (!string.IsNullOrWhiteSpace(parsed.PoNo))
                {
                    order.ErpPoNumber = parsed.PoNo;
                }
                // Save ERP PO Number
                if (!string.IsNullOrWhiteSpace(parsed.PoNo))
                {
                    order.ErpPoNumber = parsed.PoNo;
                }

                // Save ERP PO Date
                if (!string.IsNullOrWhiteSpace(parsed.PoDate) &&
                    DateTime.TryParseExact(
                        parsed.PoDate,
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var poDate))
                {
                    order.ErpPoDate = poDate;
                }
                // Save ERP submission date
                order.ErpSubmittedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    orderId = request.OrderId,
                    erpSubmitted = true,
                    confirmMessage = parsed.ConfirmMessage,
                    poNo = parsed.PoNo,
                    poDate = parsed.PoDate
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "ERP HTTP error for Warranty Order {OrderId}",
                    request.OrderId);

                return StatusCode(
                    502,
                    new
                    {
                        orderId = request.OrderId,
                        erpSubmitted = false,
                        message =
                            $"Unable to communicate with ERP: {ex.Message}"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending Warranty Order {OrderId} to ERP",
                    request.OrderId);

                return StatusCode(
                    500,
                    new
                    {
                        orderId = request.OrderId,
                        erpSubmitted = false,
                        message =
                            $"An error occurred while sending Warranty Order {request.OrderId} to ERP: {ex.Message}"
                    });
            }
        }

        private async Task<(ErpPurchaseOrderRequest Payload, WarrantyOrder Order)> BuildErpPayload(int orderId)
        {
            var order = await _context.WarrantyOrders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                throw new InvalidOperationException(
                    $"Warranty Order with Id {orderId} not found.");

            var gridRows = await _context.WarrantyOrderGridDetails
                .Where(g =>
                    g.WarrantyOrderHeaderId == orderId &&
                    g.ItemType == "Part")
                .ToListAsync();

            if (gridRows.Count == 0)
                return (
                    new ErpPurchaseOrderRequest(),
                    order
                );

            decimal headerAmount = 0;

            var poLines = new List<ErpPoLineViewModel>();

            foreach (var g in gridRows)
            {
                decimal totalTax =
                    (g.CgstAmount ?? 0) +
                    (g.SgstAmount ?? 0) +
                    (g.IgstAmount ?? 0);

                decimal qty = g.Quantity ?? 0;
                decimal totalAmount = g.TotalAmount ?? 0;

                decimal assValue = totalAmount - totalTax;

                decimal rate = qty > 0
                    ? Math.Round(assValue / qty, 2)
                    : 0;

                headerAmount += assValue;

                poLines.Add(new ErpPoLineViewModel
                {
                    ItemName = Truncate(g.PartCode, 50),
                    Descriptions = Truncate(g.PartDescription, 200),
                    Unit = "NOS",
                    Qty = qty.ToString("0.##", CultureInfo.InvariantCulture),
                    Rate = rate.ToString("0.00", CultureInfo.InvariantCulture),
                    AssValue = assValue.ToString("0.00", CultureInfo.InvariantCulture)
                });
            }

            // Generate / reuse UniqueId
            if (!order.ErpUniqueId.HasValue)
            {
                order.ErpUniqueId = await GetNextErpUniqueIdAsync();
                await _context.SaveChangesAsync();
            }

            int uniqueId = order.ErpUniqueId.Value;

            var payload = new ErpPurchaseOrderRequest
            {
                PoHeader = new ErpPoHeaderViewModel
                {
                    SupplierCode = "",   // reverted - send blank
                    RefNo = uniqueId.ToString(CultureInfo.InvariantCulture),
                    Remark = Truncate("Warranty order for item", 100),
                    Amount = headerAmount.ToString("0.00", CultureInfo.InvariantCulture),
                   // UniqueId = uniqueId
                },
                PoLine = poLines
            };

            return (payload, order);
        }

        private static string Truncate(string? value, int maxLen) =>
         string.IsNullOrEmpty(value) || value.Length <= maxLen
             ? value ?? ""
             : value[..maxLen];

        private async Task<int> GetNextErpUniqueIdAsync()
        {
            var connection = _context.Database.GetDbConnection();

            var shouldClose = connection.State != System.Data.ConnectionState.Open;

            if (shouldClose) await connection.OpenAsync();

            try
            {
                using var cmd =
                    connection.CreateCommand();

                cmd.CommandText =
                    "SELECT NEXT VALUE FOR dbo.ErpUniqueIdSequence";

                var result =
                    await cmd.ExecuteScalarAsync();

                return Convert.ToInt32(result);
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        private async Task<(string ResponseBody, ErpPurchaseOrderResponse? Parsed)> PostToErpAsync(ErpPurchaseOrderRequest payload)
        {
            var baseUrl = _configuration["ErpIntegration:BaseUrl"]
                ?? "https://uatbaplai-cpapc4h7gvdkfxh4.centralindia-01.azurewebsites.net";


            var path = _configuration["ErpIntegration:WarrantyDataPath"]
                ?? "/api/RRERPWPOHeaderLine";

            var requestUrl = $"{baseUrl.TrimEnd('/')}{path}";
            _logger.LogInformation("ERP request URL: {Url}", requestUrl);

            const int maxAttempts = 5;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                using var client = new HttpClient();

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation("ERP Warranty Order Payload: {Payload}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, content);

                var responseBody = await response.Content.ReadAsStringAsync();

                await LogApiTrackingAsync(
                    "WarrantyOrder/UATWarrantyData",
                    json,
                    $"{(int)response.StatusCode} (attempt {attempt})",
                    responseBody);

                ErpPurchaseOrderResponse? parsed = null;

                try
                {
                    parsed = JsonSerializer.Deserialize<ErpPurchaseOrderResponse>(responseBody);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid ERP response: {Response}", responseBody);
                    return (responseBody, null);
                }

                bool isDuplicateId = parsed != null && !parsed.Succeed &&
                    (parsed.ConfirmMessage?.Contains("already exist", StringComparison.OrdinalIgnoreCase) ?? false);

                if (!isDuplicateId || attempt == maxAttempts)
                    return (responseBody, parsed);

                // ERP says this UniqueId is already taken - mint a fresh one and retry.
                var newUniqueId = await GetNextErpUniqueIdAsync();
                //payload.PoHeader.UniqueId = newUniqueId;
                payload.PoHeader.RefNo = newUniqueId.ToString(CultureInfo.InvariantCulture);
            }

            throw new InvalidOperationException(
                "Failed to submit Warranty Order to ERP after retrying with new UniqueIds.");
        }

        private async Task LogApiTrackingAsync( string endpoint, string? payload, string? status, string? response)
        {
            try
            {
                await _context.Database
                    .ExecuteSqlInterpolatedAsync($@"
                INSERT INTO APITracking
                (
                    endpoint,
                    dateofhit,
                    payload,
                    status,
                    response
                )
                VALUES
                (
                    {endpoint},
                    {DateTime.Now},
                    {payload},
                    {status},
                    {response}
                )");
            }
            catch (Exception ex)
            {
                // Tracking failure must not fail the ERP transaction
                _logger.LogError(
                    ex,
                    "Failed to write APITracking row");
            }
        }
    }
}