using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ErpIntegration
{
    public class ErpIntegrationService : IErpIntegrationService
    {
        private readonly ILogger<ErpIntegrationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly BapldmsvadContext _context;

        public ErpIntegrationService(ILogger<ErpIntegrationService> logger, IConfiguration configuration, BapldmsvadContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
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
                _logger.LogError(ex, "Failed to write APITracking row for endpoint {Endpoint}", endpoint);
            }
        }

        public async Task<ErpHttpSubmitResult> SubmitWarrantyClaimLines(int invoiceId, ErpWarrantyClaimSubmitRequest payload)
        {
            const string endpoint = "WarrantyInvoice/UATWarrantyData";

            var baseUrl = _configuration["ErpIntegration:BaseUrl"]
                ?? "https://uatbaplai-cpapc4h7gvdkfxh4.centralindia-01.azurewebsites.net";
            var path = _configuration["ErpIntegration:WarrantyDataPath"] ?? "/api/UATWarrantyData";
            var requestUrl = $"{baseUrl.TrimEnd('/')}{path}";

            var checkAckResponse = _configuration.GetValue("ErpIntegration:CheckAckResponse", true);


            var json = JsonSerializer.Serialize(payload);
            _logger.LogDebug("ERP warranty payload for invoice {InvoiceId}: {Payload}", invoiceId, json);

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUrl, content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("ERP submission failed for invoice {InvoiceId} with status {StatusCode}: {Body}",
                        invoiceId, (int)response.StatusCode, body);
                    await LogApiTrackingAsync(endpoint, json, $"{(int)response.StatusCode} - HTTP error", body);
                    return new ErpHttpSubmitResult
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        ResponseBody = body,
                        ErrorMessage = $"ERP returned {(int)response.StatusCode}."
                    };
                }

                ErpAckResponse? ack = null;
                try
                {
                    ack = JsonSerializer.Deserialize<ErpAckResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException)
                {
                    // Not the {Succeed, UniqueId, ConfirmMessage} shape.
                }

                if (checkAckResponse && ack != null && !ack.Succeed)
                {
                    _logger.LogError("ERP rejected submission for invoice {InvoiceId}: {ConfirmMessage}", invoiceId, ack.ConfirmMessage);
                    await LogApiTrackingAsync(endpoint, json, "200 - Rejected (Succeed:false)", body);
                    return new ErpHttpSubmitResult
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        ResponseBody = body,
                        ErrorMessage = ack.ConfirmMessage ?? "ERP rejected the submission.",
                        UniqueId = ack.UniqueId
                    };
                }

                if (!checkAckResponse)
                    _logger.LogWarning("ERP responded 200 for invoice {InvoiceId} - CheckAckResponse is disabled, not inspecting Succeed/ConfirmMessage: {Body}",
                        invoiceId, body);

                await LogApiTrackingAsync(endpoint, json, checkAckResponse ? "200 - Success" : "200 - Not checked (ack disabled)", body);

                return new ErpHttpSubmitResult
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    ResponseBody = body,
                    UniqueId = ack?.UniqueId
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "ERP submission network error for invoice {InvoiceId}", invoiceId);
                await LogApiTrackingAsync(endpoint, json, "Network error", ex.Message);
                return new ErpHttpSubmitResult { Success = false, StatusCode = 0, ErrorMessage = $"Network error contacting ERP: {ex.Message}" };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "ERP submission timed out for invoice {InvoiceId}", invoiceId);
                await LogApiTrackingAsync(endpoint, json, "Timeout", ex.Message);
                return new ErpHttpSubmitResult { Success = false, StatusCode = 0, ErrorMessage = "ERP request timed out." };
            }
        }

        private class ErpAckResponse
        {
            public bool Succeed { get; set; }
            public string? UniqueId { get; set; }
            public string? ConfirmMessage { get; set; }
        }
    }
}