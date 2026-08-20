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

        [HttpGet("downloadExcel")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var fileBytes = await _prefixService.DownloadExcel();

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ColorMasterExcel.xlsx"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while downloading color excel");
                throw;
            }
        }

        [HttpGet("GetPrefixByPagedByDealer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPrefixByPagedByDealer(
            [FromQuery] string? searchTerm,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize,
            [FromQuery] string? dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var prefixs = await _prefixService.GetPrefixByPagedByDealer(pageIndex, pageSize, searchTerm, dealerCode);

                return Ok(prefixs);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("byId/{id}")]
        [ProducesResponseType(typeof(NumberSequence), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _prefixService.GetById(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching prefix by id: {ex.Message}");
                throw;
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePrefix(int id, NumberSequenceViewModel numberSequenceViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _prefixService.UpdatePrefix(id, numberSequenceViewModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating prefix: {ex.Message}");
                throw;
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePrefix(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _prefixService.DeletePrefix(id);
                if (!result) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting prefix: {ex.Message}");
                throw;
            }
        }

        [HttpGet("checkDuplicate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckDuplicate(
            [FromQuery] string dealerCode,
            [FromQuery] string moduleName,
            [FromQuery] string year,
            [FromQuery] string prefix,
            [FromQuery] int? excludeId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _prefixService.CheckDuplicate(dealerCode, moduleName, year, prefix, excludeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while checking duplicate: {ex.Message}");
                throw;
            }
        }
    }
}
