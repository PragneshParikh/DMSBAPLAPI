using DMS_BAPL_Data.Services.LOTInspectionService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LOTInspectionController : ControllerBase
    {
        private readonly ILotInspectionService _invoiceService;
        private readonly ILotInspectionDetailsService _lotInspectionDetailsService;
        public LOTInspectionController(ILotInspectionService invoiceService,
            ILotInspectionDetailsService lotInspectionDetailsService)
        {
            _invoiceService = invoiceService;
            _lotInspectionDetailsService = lotInspectionDetailsService;
        }
        
        [HttpPost]
        [Route("AcceptInvoices")]
        public async Task<IActionResult> AcceptInvoices([FromBody] string invoiceNo)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found");

                var result = await _invoiceService.InsertLotInspectionHeaderAsync(invoiceNo, userId);

                if (result > 0)
                {
                    await _lotInspectionDetailsService.InsertLotDetailsByInvoiceNo(invoiceNo, result, userId);
                }

                return Ok(new
                {
                    Message = "Invoices inserted successfully",
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
