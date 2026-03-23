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
        /// <summary>
        /// Creates a new dealer.
        /// </summary>
        /// <param name="dealer">Dealer data</param>
        /// <returns>Created dealer details</returns>

        [HttpPost("create")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDealer([FromBody] DealerMasterViewModel dealer)
        {
            try
            {
                if (dealer == null)
                    return BadRequest(StringConstants.BadRequest);

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(StringConstants.UserUnauthorized);

                var result = await _dealerMasterService.AddDealerAsync(dealer, userId);

                return Ok(new
                {
                    message = StringConstants.DealerCreated,
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
        /// <summary>
        /// Retrieves all dealers with optional search filter.
        /// </summary>
        /// <param name="search">Search keyword</param>
        /// <returns>List of dealers</returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDealers(string? search)
        {
            try
            {

                var dealers = await _dealerMasterService.GetAllDealersAsync(search);

                return Ok(new
                {
                    message = StringConstants.DealerFetched,
                    data = dealers ?? new List<DealerMaster>()
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

        /// <summary>
        /// Retrieves dealer by ID.
        /// </summary>
        /// <param name="id">Dealer ID</param>
        /// <returns>Dealer details</returns>

        [HttpGet("dealer")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDealerById(int id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }

        }

        /// <summary>
        /// Retrieves dealer by dealer code.
        /// </summary>
        /// <param name="dealerCode">Dealer code</param>
        /// <returns>Dealer details</returns>

        [HttpGet("dealerCode")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDealerByDealerCode(string dealerCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dealerCode))
                    return BadRequest(StringConstants.BadRequest);

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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }

        }
        /// <summary>
        /// Updates dealer information.
        /// </summary>
        /// <param name="id">Dealer ID</param>
        /// <param name="dealer">Updated dealer data</param>
        /// <returns>Updated dealer details</returns>
        [HttpPut("update/{id}")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDealer(int id, [FromBody] DealerMasterViewModel dealer)
        {
            try
            {
                if (dealer == null)
                    return BadRequest(StringConstants.BadRequest);
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(StringConstants.UserUnauthorized);
                var result = await _dealerMasterService.UpdateDealerAsync(id, dealer, userId);

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

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Downloads dealer list as an Excel file.
        /// </summary>
        /// <returns>Excel file containing dealer data</returns>
        [HttpGet("download")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download()
        {
            try
            {

                var file = await _dealerMasterService.DownloadDealerExcel();

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "DealerList.xlsx"
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
        /// <summary>
        /// Retrieves dealer dropdown list (for UI selection).
        /// </summary>
        /// <returns>List of dealers for dropdown</returns>

        [HttpGet("getDealerDropdown")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDealerDropdown()
        {
            try
            {
                var result = await _dealerMasterService.GetDealerDropdown();

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
