using DMS_BAPL_Data.Repositories.InvoiceDispatchRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceDispatchController : ControllerBase
    {
        private readonly IInvoiceDispatchRepo _invoiceDispatchRepo;

        public InvoiceDispatchController(IInvoiceDispatchRepo invoiceDispatchRepo)
        {
            _invoiceDispatchRepo = invoiceDispatchRepo;
        }

        // GET api/InvoiceDispatch/parts?dealerCode=&fromDate=&toDate=&pageIndex=1&pageSize=25
        [HttpGet("parts")]
        public async Task<IActionResult> GetPartDispatchList([FromQuery] InvoiceDispatchViewModel filter)
        {
            try
            {
                var result = await _invoiceDispatchRepo.GetPartDispatchList(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch part dispatch list.", Error = ex.Message });
            }
        }

        // GET api/InvoiceDispatch/vehicles?dealerCode=&fromDate=&toDate=&pageIndex=1&pageSize=25
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicleDispatchList([FromQuery] InvoiceDispatchViewModel filter)
        {
            try
            {
                var result = await _invoiceDispatchRepo.GetVehicleDispatchList(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch vehicle dispatch list.", Error = ex.Message });
            }
        }
    }
}
