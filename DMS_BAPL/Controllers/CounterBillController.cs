using DMS_BAPL_Data.Services.CounterBillService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CounterBillController : ControllerBase
    {
        private readonly ICounterBillService _counterBillService;
        public CounterBillController(ICounterBillService counterBillService)
        {
            _counterBillService = counterBillService;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] CounterBillViewModel model)
        {
            var userName = User.Identity?.Name ?? "Admin";
            var id = await _counterBillService.SaveCounterBillAsync(model, userName);

            return Ok(new
            {
                Success = true,
                CounterBillId = id,
                Message = "Counter Bill Saved Successfully"
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] CounterBillViewModel model, int id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "Admin";

                var result = await _counterBillService.UpdateCounterBillAsync(model, userName, id);

                return Ok(new
                {
                    Success = true,
                    CounterBillId = id,
                    Message = "Counter Bill Updated Successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCounterBillById(int id)
        {
            try
            {
                var result = await _counterBillService.GetCounterBillById(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Counter Bill not found"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllCounterBills([FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? search, [FromQuery] string? dealerFilter)
        {
            try
            {
                var result = await _counterBillService.GetAllCounterBills(dealerCode, fromDate, toDate, search,dealerFilter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCounterBill(int id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";

                var result = await _counterBillService.DeleteCounterBill(id, userName);

                if (!result)
                    return NotFound(new { message = "Counter Bill not found." });

                return Ok(new
                {
                    success = true,
                    message = "Counter Bill deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("download-counter-bill-excel")]
        public async Task<IActionResult> DownloadCounterBillExcel(string? dealerCode, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                var fileBytes = await _counterBillService.DownloadCounterBillExcel(
                    dealerCode,
                    dateFrom,
                    dateTo);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"CounterBillReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
