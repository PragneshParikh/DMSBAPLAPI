using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, Dealer")]
    public class DealerMasterController : ControllerBase
    {
        private readonly IDealerMasterService _dealerMasterService;

        public DealerMasterController(IDealerMasterService dealerMasterService)
        {
            _dealerMasterService = dealerMasterService;

        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDealer([FromBody] DealerMasterViewModel dealer)
        {
            string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

            if (string.IsNullOrEmpty(userId))
                return BadRequest("User not found");

            var result = await _dealerMasterService.AddDealerAsync(dealer, userId);

            return Ok(new
            {
                message = StringConstants.DealerCreated,
                data = result
            });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllDealers(string? search)
        {
            var dealers = await _dealerMasterService.GetAllDealersAsync(search);

            return Ok(new
            {
                message = StringConstants.DealerFetched,
                data = dealers ?? new List<DealerMaster>()
            });
        }

        [HttpGet("dealer")]
        public async Task<IActionResult> GetDealerById(int id)
        {
            var dealer = await _dealerMasterService.GetDealerById(id);

            if (dealer == null)
            {
                return NotFound(StringConstants.DealerNotFound);
            }
            return Ok(new
            {
                message = StringConstants.DealerFetched,
                data = dealer
            });

        }

        [HttpGet("dealerCode")]
        public async Task<IActionResult> GetDealerByDealerCode(string dealerCode)
        {
            var dealer = await _dealerMasterService.GetDealerByCode(dealerCode);

            if (dealer == null)
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
        public async Task<IActionResult> UpdateDealer(int id, [FromBody] DealerMasterViewModel dealer)
        {
            string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

            if (string.IsNullOrEmpty(userId))
                return BadRequest("User not found");
            var result = await _dealerMasterService.UpdateDealerAsync(id, dealer,userId);

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

        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {
            var file = await _dealerMasterService.DownloadDealerExcel();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DealerList.xlsx"
            );
        }

        [HttpGet("getDealerDropdown")]
        public async Task<IActionResult> GetDealerDropdown()
        {
            var result = await _dealerMasterService.GetDealerDropdown();
            return Ok(result);
        }
    }
}
