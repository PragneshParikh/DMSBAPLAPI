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
    }
}
