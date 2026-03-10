using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerMasterController : ControllerBase
    {
        private readonly IDealerMasterService _dealerMasterService;

        public DealerMasterController(IDealerMasterService dealerMasterService)
        {
            _dealerMasterService = dealerMasterService;
            
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDealer([FromBody] DealerMasterDto dealer)
        {
            var result = await _dealerMasterService.AddDealerAsync(dealer);

            return Ok(new
            {
                message = StringConstants.DealerCreated,
                data = result
            });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllDealers()
        {
            var dealers = await _dealerMasterService.GetAllDealersAsync();

            if (dealers == null || dealers.Count == 0)
            {
                return NotFound(StringConstants.DealerNotFound);
            }

            return Ok(new
            {
                message = StringConstants.DealerFetched,
                data = dealers
            });
        }

        [HttpGet("dealer")]
        public async Task<IActionResult> GetDealerById(int id)
        {
            var dealer = await _dealerMasterService.GetDealerById(id);
            
            if(dealer == null)
            {
                return NotFound(StringConstants.DealerNotFound);
            }
            return Ok(new
            {
                message = StringConstants.DealerFetched,
                data = dealer
            });

        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDealer(int id, [FromBody] DealerMasterDto dealer)
        {
            var result = await _dealerMasterService.UpdateDealerAsync(id, dealer);

            if (result == null)
            {
                return NotFound(StringConstants.DealerUpdateFailed);
            }

            return Ok(new
            {
                message = StringConstants.DealerUpdated,
                data = result
            });
        }
    }
}
