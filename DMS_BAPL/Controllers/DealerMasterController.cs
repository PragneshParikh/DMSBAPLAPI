using DMS_BAPL_Data.CustomModel;
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
                throw;
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
                throw;
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
                throw;
            }

        }

        /// <summary>
        /// Retrieves dealer by dealer code.
        /// </summary>
        /// <param name="dealerCode">Dealer code</param>
        /// <returns>Dealer details</returns>

        [HttpGet("GetByDealerCode/{dealerCode}")]
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
                throw;
            }

        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponse<DealerMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<DealerMaster>>> GetDealerByPagedAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? dealerCode = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var delaers = await _dealerMasterService.GetDealerByPaged(searchTerm, pageIndex, pageSize, dealerCode);

                return Ok(delaers);
            }
            catch (Exception ex)
            {
                throw;
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
                throw;
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
                throw;
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
        public async Task<IActionResult> GetDealerDropdown([FromQuery] string? dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _dealerMasterService.GetDealerDropdown(dealerCode);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut("updateTradeCertificate")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTradeCertificate(string dealerCode, [FromBody] string tradeCertificate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tradeCertificate))
                    return BadRequest(StringConstants.BadRequest);
                var result = await _dealerMasterService.EditTradeCertificate(dealerCode, tradeCertificate);
                if (result == null)
                {
                    return NotFound(StringConstants.DealerNotFound);
                }
                return Ok(new
                {
                    message = StringConstants.TradeCertificateUpdated,
                    data = result
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Insert/Update the data from the ERP
        /// </summary>
        /// <param name="dealerMaster"></param>
        /// <returns></returns>
        [HttpPut("UpdateByDealerCode")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> UpdateByDealerCode([FromBody] DealerMasterViewModel dealerMaster)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var dealer = await _dealerMasterService.UpdateByDealerCode(userId, dealerMaster);

                if (dealer == null)
                    return NotFound(StringConstants.DealerNotFound);

                return Ok(new
                {
                    message = StringConstants.DealerUpdated,
                    data = dealer
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
