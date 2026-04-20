using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.LedgerMasterService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/ledger-master")]
    [ApiController]
    public class LedgerMasterController : ControllerBase
    {
        private readonly ILedgerMasterService _ledgerMasterService;
        private readonly ILogger<LedgerMasterController> _logger;

        public LedgerMasterController(ILedgerMasterService ledgerMasterService, ILogger<LedgerMasterController> logger)
        {
            _ledgerMasterService = ledgerMasterService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LedgerMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LedgerMaster>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledger = _ledgerMasterService.GetAll();

                return Ok(ledger);
            }
            catch (Exception)
            {
                _logger.LogError("");
                throw;
            }
        }

        [HttpGet("Paged")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetLedgerByPagedAsync(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledger = await _ledgerMasterService.GetLedgerByPagedAsync(searchTerm, pageIndex, pageSize);

                return Ok(ledger);
            }
            catch (Exception)
            {
                _logger.LogError("");
                throw;
            }
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(LedgerMaster), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LedgerMaster?>> GetLedgerMasterById(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledger = await _ledgerMasterService.GetLedgerByIdAsync(Id);

                return Ok(ledger);
            }
            catch
            {
                _logger.LogError("");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> InsertLedgerDetails(LedgerMaster ledgerMaster)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (ledgerMaster is null)
                    return BadRequest("Invalid data");

                var ledger = await _ledgerMasterService.InsertLedgerDetail(ledgerMaster, userId);

                return Ok(ledger);
            }
            catch { throw; }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateLedgerDetails(LedgerMaster ledgerMaster)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (ledgerMaster is null)
                    return BadRequest("Invalid data");

                var ledger = await _ledgerMasterService.UpdateLedgerDetail(ledgerMaster, userId);

                return Ok(ledger);
            }
            catch { throw; }
        }

    }
}
