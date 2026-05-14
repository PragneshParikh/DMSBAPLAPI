using DMS_BAPL_Data.Services.TaxServices;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/tax")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        private readonly ITaxServices _taxServices;
        private readonly ILogger<TaxController> _logger;

        public TaxController(ITaxServices taxServices, ILogger<TaxController> logger)
        {
            _taxServices = taxServices;
            _logger = logger;
        }

        [HttpGet("GetTax")]
        [ProducesResponseType(typeof(List<TaxDetailViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTaxByHSNCode(
            [FromQuery] string? itemCode,
            [FromQuery] string? dealerLocation
            )
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var taxDetails = await _taxServices.GetTaxDetailsAsync(itemCode, dealerLocation, null);

                return Ok(taxDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching tax details for itemCode: {itemCode}, dealerLocation: {dealerLocation}, customerLocation: {null}", itemCode, dealerLocation);
                throw;
            }
        }
    }
}
