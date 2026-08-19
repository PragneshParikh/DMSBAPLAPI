using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Services.ErpIntegration
{
    // PLACEHOLDER IMPLEMENTATION. The real submit endpoint URL, its exact
    // request/response shape, and the token retrieval mechanism are all
    // UNCONFIRMED - only the GET report-fetch contract was provided. Do
    // not treat this as production-ready until those are confirmed against
    // real ERP documentation/testing.
    public class ErpIntegrationService : IErpIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ErpIntegrationService> _logger;

        public ErpIntegrationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ErpIntegrationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ErpSubmitResult> SubmitWarrantyClaimLines(ErpWarrantyClaimSubmitRequest request)
        {
            // CONFIRM: real submit URL. This config key currently has no
            // real value set - see appsettings.json snippet below.
            var submitUrl = _configuration["ErpIntegration:SubmitWarrantyClaimUrl"];
            var authToken = _configuration["ErpIntegration:AuthToken"];

            if (string.IsNullOrWhiteSpace(submitUrl) || string.IsNullOrWhiteSpace(authToken))
            {
                return new ErpSubmitResult
                {
                    Success = false,
                    Message = "ERP integration is not configured - missing SubmitWarrantyClaimUrl or AuthToken in configuration.",
                    LinesSent = 0
                };
            }

            var client = _httpClientFactory.CreateClient("ErpApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", authToken);

            try
            {
                var response = await client.PostAsJsonAsync(submitUrl, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ERP submit failed with status {Status}: {Body}", response.StatusCode, errorBody);
                    return new ErpSubmitResult
                    {
                        Success = false,
                        Message = $"ERP returned {(int)response.StatusCode}: {errorBody}",
                        LinesSent = 0
                    };
                }

                // Response shape here is UNCONFIRMED for a submit endpoint -
                // reusing the documented { Valid, Description, Value }
                // envelope defensively, matching the GET report's own shape.
                var result = await response.Content.ReadFromJsonAsync<ErpApiResponse<object>>();

                return new ErpSubmitResult
                {
                    Success = result?.Valid ?? false,
                    Message = result?.Description ?? "No description returned by ERP.",
                    LinesSent = request.Value.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while submitting Warranty Claim lines to ERP.");
                return new ErpSubmitResult
                {
                    Success = false,
                    Message = $"Exception while calling ERP: {ex.Message}",
                    LinesSent = 0
                };
            }
        }
    }
}