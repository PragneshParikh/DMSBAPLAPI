using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.ReceiptEntryService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptEntryController : ControllerBase
    {
        private readonly IReceiptEntryService _receiptEntryService;
        public ReceiptEntryController(IReceiptEntryService receiptEntryService)
        {
            _receiptEntryService = receiptEntryService;
        }

        /// <summary>
        /// Retreives the next receipt no sequence
        /// </summary>
        /// <returns>Next receipt No</returns>
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("getNextReceiptNo")]
        public async Task<IActionResult> GetNextReceiptNo()
        {
            try
            {
                var receiptNo = await _receiptEntryService.GenerateNextReceiptNoAsync();
                return Ok(receiptNo);

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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("getReceiptEntryList")]
        public async Task<IActionResult> GetReceiptEntryListAsync([FromQuery] ReceiptFilterViewModel filter)
        {
            try
            {
                var result = await _receiptEntryService.GetReceiptEntryListAsync(filter);
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("ledgerList")]
        public async Task<IActionResult> GetLedgerMasterDetailsByTypeAsync(string ledgerType)
        {
            try
            {
                var result = await _receiptEntryService.GetLedgerMasterDetailsByTypeAsync(ledgerType);
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("addReceiptEntry")]
        public async Task<IActionResult> AddReceiptEntryAsync(ReceiptEntryViewModel receiptEntry)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(StringConstants.UserUnauthorized);

                var result = await _receiptEntryService.AddReceiptEntryAsync(receiptEntry, userId);
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


        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("editReceiptEntry")]

        public async Task<IActionResult?> UpdateReceiptEntryAsync(int id, ReceiptEntryViewModel receiptEntry)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                var result = await _receiptEntryService.UpdateReceiptEntryAsync(id, receiptEntry, userId);
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("receiptById")]

        public async Task<IActionResult> GetReceiptByIdAsync(int id)
        {
            try
            {
                var result = await _receiptEntryService.GetReceiptByIdAsync(id);
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

        [HttpGet("checkLeadExist")]
        public async Task<IActionResult> GetLMSLeadByMoborBookingId(string? mobileNo, string? bookingId)
        {
            try
            {
                var item = await _receiptEntryService.CheckReceiptExist(mobileNo, bookingId);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("download")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> downloadExcel()
        {
            try
            {

                var file = await _receiptEntryService.downloadReceiptExcel();

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ReceiptEntry.xlsx"
                );
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("getAllReceiptList")]
        public async Task<IActionResult> GetReceiptEntryListAsyncWithSearch(string? searchTerm, DateOnly? fromDate, DateOnly? toDate)
        {
            try
            {
                var result = await _receiptEntryService.GetReceiptEntryListAsyncWithSearch(searchTerm,fromDate, toDate);
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
