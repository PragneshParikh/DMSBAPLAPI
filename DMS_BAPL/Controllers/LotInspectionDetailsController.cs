using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.LOTInspectionService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotInspectionDetailsController : ControllerBase
    {
        private readonly ILotInspectionDetailsService _lotInspectionDetailsService;

        public LotInspectionDetailsController(ILotInspectionDetailsService lotInspectionDetailsService)
        {
            _lotInspectionDetailsService = lotInspectionDetailsService;
        }

        [HttpPost]
        [Route("InsertDetailsByInvoice")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertDetailsByInvoice([FromBody] InsertDetailsByInvoiceViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found");

                var result = await _lotInspectionDetailsService.InsertDetailsByInvoiceAsync(model, userId);

                return Ok(new
                {
                    Message = "Invoice based model Details inserted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllDetailsByInvoice")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDetailsByInvoice([FromQuery] string invoiceNo)
        {
            try
            {
                var result = await _lotInspectionDetailsService.GetAllDetailsByInvoiceAsync(invoiceNo);
                return Ok(new
                {
                    Message = "Invoice based model Details retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
