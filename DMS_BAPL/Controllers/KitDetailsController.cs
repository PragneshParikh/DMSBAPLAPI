using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.KitDetailsService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/kit-details")]
    [ApiController]
    public class KitDetailsController : ControllerBase
    {
        private readonly IKitDetailsService _kitDetailsService;
        private readonly ILogger<KitDetailsController> _logger;

        public KitDetailsController(IKitDetailsService kitDetailsService, ILogger<KitDetailsController> logger)
        {
            _kitDetailsService = kitDetailsService;
            _logger = logger;
        }

        [HttpGet("{headerId}")]
        [ProducesResponseType(typeof(IEnumerable<KitDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<KitDetail>>> GetByHeaderId(int headerId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var kitDetails = await _kitDetailsService.GetKitDetailsByHeaderId(headerId);

                return Ok(kitDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error: {ex.Message}");
                throw;
            }

        }

        [HttpGet("Paged")]
        [ProducesResponseType(typeof(IEnumerable<KitDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<KitDetail>>> GetKitDetailsByPaged(
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize,
            [FromQuery] int headerId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var kitDetails = await _kitDetailsService.GetKitDetailsByPaged(pageIndex, pageSize, headerId);

                return Ok(kitDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching kit details : {ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> InsertKitDetails(List<KitDetailsViewModel> kitDetailViewModel)
        {
            if (kitDetailViewModel == null || !kitDetailViewModel.Any())
                return BadRequest("Kit details cannot be empty.");

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _kitDetailsService.InsertKitDetails(kitDetailViewModel);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while inserting kit details");
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> UpdateKitDetails(List<KitDetailsViewModel> kitDetailsViewModels)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var _kit = await _kitDetailsService.UpdateKitDetails(kitDetailsViewModels);

                return Ok(_kit);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in updatint kit details records : {ex.Message}");
                throw;
            }
        }

    }
}
