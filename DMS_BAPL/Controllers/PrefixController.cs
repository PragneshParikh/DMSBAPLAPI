using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/prefix")]
    [ApiController]
    public class PrefixController : ControllerBase
    {
        private readonly IPrefixService _prefixService;
        private readonly ILogger<PrefixController> _logger;

        public PrefixController(IPrefixService prefixService, ILogger<PrefixController> logger)
        {
            _prefixService = prefixService;
            _logger = logger;
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponse<NumberSequence>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<NumberSequence>>> GetPrefixesByPaged(string? searchTerm, int pageIndex, int pageSize)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefixes = await _prefixService.GetPrefixByPagedAsync(searchTerm, pageIndex, pageSize);
                return Ok(prefixes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving prefixes.{ex.Message}");
                throw;
            }
        }

        [HttpGet("{dealerCode}")]
        [ProducesResponseType(typeof(IEnumerable<NumberSequence>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NumberSequence>>> GetPrefixByDealerCode(string dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefix = await _prefixService.GetPrefixByDealerCode(dealerCode);

                return Ok(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving prefixes by dealer : ${ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertPrefix(NumberSequence numberSequence)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefix = await _prefixService.InsertPrefix(numberSequence);

                return Ok(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while creating prefix: {ex.Message}");
                throw;
            }
        }
    }
}
