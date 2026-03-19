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
        public async Task<IActionResult> GetHsncodeList()
        {
            var result = await _hsnWiseTaxcodeservice.GetHsncodeList();
            return Ok(result);
        }

        [HttpGet("GetAggregateTaxCodeList")]
        public async Task<IActionResult> GetAggregateTaxCodeList()
        {
            var result = await _hsnWiseTaxcodeservice.GetAggregateTaxCodeList();
            return Ok(result);
        }

        [HttpPost("InsertHsnwiseTaxcodedetails")]
        public async Task<IActionResult> InsertHsnwiseTaxcodedetails([FromBody] HsnwiseTaxCodeViewModel hsnwiseTaxCodeViewModel)
        {
            var result = await _hsnWiseTaxcodeservice.InsertHsnwiseTaxcodedetails(hsnwiseTaxCodeViewModel);
            return Ok(result);

        }

        [HttpGet("GetHsnwiseTaxcodedetails")]
        public async Task<IActionResult> GetHsnwiseTaxcodedetails(string? search)
        {
            var result = await _hsnWiseTaxcodeservice.GetHsnwiseTaxcodedetails(search);
            return Ok(result);
        }
    }
}
