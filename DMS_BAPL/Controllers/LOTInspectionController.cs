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
        public LOTInspectionController(ILotInspectionService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        [HttpPost]
        [Route("AcceptInvoices")]
        public async Task<IActionResult> AcceptInvoices(string invoiceNo)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found");

                var result = await _invoiceService.InsertLotInspectionHeaderAsync(invoiceNo,userId);

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
