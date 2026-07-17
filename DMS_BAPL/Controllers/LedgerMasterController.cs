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
        public async Task<ActionResult<IEnumerable<object>>> GetLedgerByPagedAsync([FromQuery] string? searchTerm = null, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? dealerCode = null, [FromQuery] string? filter = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledger = await _ledgerMasterService.GetLedgerByPagedAsync(searchTerm, pageIndex, pageSize, dealerCode, filter);

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

        [HttpGet("companies")]
        [ProducesResponseType(typeof(IEnumerable<LedgerMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LedgerMaster>>> GetCompanyLedgers()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledgers = await _ledgerMasterService.GetCompanyLedgersAsync();

                return Ok(ledgers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company ledgers");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }

        [HttpGet("insurance")]
        [ProducesResponseType(typeof(IEnumerable<LedgerMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LedgerMaster>>> GetInsuranceLedgers()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var ledgers = await _ledgerMasterService.GetInsuranceLedgersAsync();
                return Ok(ledgers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting insurance ledgers");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }

        }
        [HttpGet("ledgerByType")]
        [ProducesResponseType(typeof(IEnumerable<LedgerMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LedgerMaster>>> GetLedgerByLedgerType(string? ledgerType)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledgers = await _ledgerMasterService.GetLedgerByLedgerType(ledgerType);

                return Ok(ledgers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company ledgers");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }

        [HttpGet("getNextLed")]
        public async Task<string> GetNextLedId(string dealerCode)
        {
            try
            {
                return await _ledgerMasterService.GetNextLedId(dealerCode);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("getLedgerMobileList")]
        public async Task<ActionResult<List<string>>> GetAllMobileNumberByDealerCode(string dealerCode)
        {
            try
            {
                return await _ledgerMasterService.GetAllMobileNumberByDealerCode(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("download")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download(string? dealerCode)
        {
            try
            {

                var file = await _ledgerMasterService.DownloadExcel(dealerCode);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "LedgerList.xlsx"
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



        [HttpGet("getLedgerForSale")]
        public async Task<List<LedgerMaster>> GetLedgerForSale(string? dealerCode, bool isSuperAdmin)
        {
            try
            {
                return await _ledgerMasterService.GetLedgerForSale(dealerCode, isSuperAdmin);
            }
            catch
            {
                throw;


            }
        }

        [HttpGet("GetLotRelatedLedgers")]
        [ProducesResponseType(typeof(IEnumerable<LedgerMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LedgerMaster>>> GetLotRelatedLedgers(string? invoiceNo, bool? IsD2D)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledgers = await _ledgerMasterService.GetLotRelatedLedgers(invoiceNo, IsD2D);

                return Ok(ledgers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company ledgers");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }

        [HttpGet("GetSupplierLedgers")]
        [ProducesResponseType(typeof(IEnumerable<LedgerMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LedgerMaster>>> GetSupplierLedgers(string? dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var ledgers = await _ledgerMasterService.GetSupplierLedgers(dealerCode);

                return Ok(ledgers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company ledgers");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }


    }
}
