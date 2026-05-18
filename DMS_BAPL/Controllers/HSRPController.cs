using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.HSRPService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HSRPController : ControllerBase
    {
        private readonly IHSRPService _hsrpService;
        public HSRPController(IHSRPService hSRPService)
        {
            _hsrpService = hSRPService;
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPendingHSRPOrders(string? dealerCode,DateTime? fromDate,DateTime? toDate)
        {
            try
            {

                var data = await _hsrpService.GetPendingHSRPListAsync(dealerCode, fromDate, toDate);

               return Ok(data);

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

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBulkHSRPOrder([FromBody] List<HSRPOrderCreateViwModel> order)
        {
            try
            {
                var data = await _hsrpService.CreateBulkHSRPOrder(order);
                return Ok(data);
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
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBulkHSRPOrder([FromBody] List<HSRPOrderCreateViwModel> order)
        {
            try
            {
                var data = await _hsrpService.UpdateBulkHSRPOrder(order);
                return Ok(data);
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

        [HttpPut("updateInward")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBulkHSRPInward([FromBody] List<HSRPInwardUpdate> order)
        {
            try
            {
                var data = await _hsrpService.UpdateInwardStatus(order);
                return Ok(data);
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
        [HttpGet("hsrpList")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHSRPListAsync(string? dealerCode,DateTime? fromDate,DateTime? toDate)
        {
            try
            {

                var data = await _hsrpService.GetAllHSRPOrderAsync(dealerCode, fromDate, toDate);

                return Ok(data);

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

        [HttpGet("hsrpInwardList")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHSRPInwardListAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                var data = await _hsrpService.GetAllHSRPInward(dealerCode, fromDate, toDate);

                return Ok(data);

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

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _hsrpService.GetAllHSRPOrderByIdAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
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
