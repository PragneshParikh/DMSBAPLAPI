using DMS_BAPL_Data.DBModels;
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
                if (request == null || string.IsNullOrEmpty(request.documentNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "VehicleSaleBillNo is required"
                    });
                }

                var result = await _service.GeneratePerformaInvoice(request.documentNo);

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceHeader invoice)
        {
            var id = await _service.CreateAsync(invoice);
            return Ok(id);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] InvoiceHeader invoice)
        {
            await _service.UpdateAsync(invoice);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }
    }
}

