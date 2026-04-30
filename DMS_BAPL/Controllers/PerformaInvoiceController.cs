using DMS_BAPL_Data.Services.PerformaInvoiceService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerformaInvoiceController : ControllerBase
    {
        private readonly IPerformaInvoiceService _service;
        public PerformaInvoiceController(IPerformaInvoiceService service)
        {
            _service = service;
        }

        [HttpPost("createPerformaInvoice")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GeneratePerformaInvoice([FromBody] PerformaInvoiceRequestViewModel request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.vehicleSaleBillNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "VehicleSaleBillNo is required"
                    });
                }

                var result = await _service.GeneratePerformaInvoice(request.vehicleSaleBillNo);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}

