using DMS_BAPL_Data.Services.EbwInvoiceService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/ebw-invoice")]
    [ApiController]
    public class EbwInvoiceController : ControllerBase
    {
        private readonly IEbwInvoiceService _service;
        public EbwInvoiceController(IEbwInvoiceService service) { _service = service; }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] EbwInvoiceSaveViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var id = await _service.SaveAsync(model, userId);
                return Ok(new { success = true, id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var result = await _service.GetAllAsync(dealerCode, fromDate, toDate);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("next-prefix")]
        public async Task<IActionResult> GetNextPrefixNo([FromQuery] string dealerCode)
        {
            try
            {
                var (prefixNo, nextNo) = await _service.GetNextPrefixNoAsync(dealerCode);
                return Ok(new { prefixNo, nextNo });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        // EbwInvoiceController.cs
        [HttpGet("report")]
        public async Task<IActionResult> GetReportData([FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var result = await _service.GetReportDataAsync(dealerCode, fromDate, toDate);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("dealer-info/{dealerCode}")]
        public async Task<IActionResult> GetDealerInfo(string dealerCode)
        {
            var result = await _service.GetDealerInfoAsync(dealerCode);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
