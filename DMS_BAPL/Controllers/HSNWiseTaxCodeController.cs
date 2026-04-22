using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.HSNWiseTaxcodeService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HSNWiseTaxCodeController : ControllerBase
    {
        private readonly IHSNWiseTaxcodeservice _hsnWiseTaxcodeservice;

        public HSNWiseTaxCodeController(IHSNWiseTaxcodeservice hsnWiseTaxcodeservice)
        {
            _hsnWiseTaxcodeservice = hsnWiseTaxcodeservice;
        }

        [HttpGet("GetHsncodeList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHsncodeList()
        {
            try
            {
                var result = await _hsnWiseTaxcodeservice.GetHsncodeList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                // For example: _logger.LogError(ex, "An error occurred while fetching HSN code list.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
                
        }

        [HttpGet("GetAggregateTaxCodeList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAggregateTaxCodeList()
        {
            try
            {
                var result = await _hsnWiseTaxcodeservice.GetAggregateTaxCodeList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                // For example: _logger.LogError(ex, "An error occurred while fetching aggregate tax code list.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost("InsertHsnwiseTaxcodedetails")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertHsnwiseTaxcodedetails([FromBody] HsnwiseTaxCodeViewModel hsnwiseTaxCodeViewModel)
        {
            try
            {
                if (hsnwiseTaxCodeViewModel == null)
                {
                    return BadRequest("Invalid input data.");
                }
                var result = await _hsnWiseTaxcodeservice.InsertHsnwiseTaxcodedetails(hsnwiseTaxCodeViewModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                // For example: _logger.LogError(ex, "An error occurred while inserting HSN wise tax code details.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GetHsnwiseTaxcodedetails")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHsnwiseTaxcodedetails(string? search)
        {
            try
            {
                var result = await _hsnWiseTaxcodeservice.GetHsnwiseTaxcodedetails(search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                // For example: _logger.LogError(ex, "An error occurred while fetching HSN wise tax code details.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
