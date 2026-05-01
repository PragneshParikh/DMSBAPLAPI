using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2013.Drawing;
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

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NumberSequence>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NumberSequence>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefix = await _prefixService.Get();

                return Ok(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching prefix list : ${ex.Message}");
                throw;
            }
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

        [HttpGet("{dealerCode}/modules/{moduleName}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetPrefixByDealerCodeModuleName(string dealerCode, string moduleName)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var sequence = await _prefixService.GetPrefixByDealerCodeModuleName(dealerCode, moduleName);

                if (sequence == null)
                    return NotFound("Sequence not found");

                string prefix = sequence.SequenceCode;
                int nextNo = sequence.NextNo;

                int digitCount = prefix.Count(c => c == '#');

                string formattedNo = nextNo.ToString().PadLeft(digitCount, '0');

                string result = prefix.Replace(new string('#', digitCount), formattedNo);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching prefix number by dealer: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("AddPrefixForDealers")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPrefixForDealers(NumberSequenceViewModel numberSequenceViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefix = await _prefixService.AddPrefixForDealers(numberSequenceViewModel);

                return Ok(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while creating prefix: {ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertPrefix(NumberSequenceViewModel numberSequenceViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefix = await _prefixService.InsertPrefix(numberSequenceViewModel);

                return Ok(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while creating prefix: {ex.Message}");
                throw;
            }
        }

        [HttpPut("{dealerCode}/modules/{moduleName}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateNextNumberByDealerByModule(string dealerCode, string moduleName)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefix = await _prefixService.UpdateNextNumberByDealerByModule(dealerCode, moduleName);

                return Ok(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating the next number : ${ex.Message}");
                throw;
            }
        }
    }
}
