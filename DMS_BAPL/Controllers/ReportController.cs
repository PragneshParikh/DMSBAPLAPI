using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.ReportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/Report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // STOCK
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get Dealer Wise Stock Report</summary>
        [HttpGet("dealer-wise")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDealerWiseReport([FromQuery] string? dealerCode = null)
        {
            try
            {
                string userId =
                    GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // Dealer users see only their stock
                if (!User.IsInRole("SuperAdmin"))
                {
                    dealerCode =
                        User.FindFirst("DealerCode")?.Value;
                }

                var result =
                    await _reportService
                        .GetDealerWiseStockReportAsync(dealerCode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching dealer wise stock report");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("model-list")]
        public async Task<IActionResult> GetModelList()
        {
            var result =
                await _reportService.GetModelListAsync();

            return Ok(result);
        }

        [HttpGet("chassis-list")]
        public async Task<IActionResult> GetChassisList()
        {
            var result =
                await _reportService.GetChassisListAsync();

            return Ok(result);
        }



        // ─────────────────────────────────────────────────────────────────────
        // JOB CARD
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get Job Card Report with pagination and filtering</summary>
        [HttpPost("job-card")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardReport([FromBody] JobReportFilterModel filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                // ── Dealer restriction: non-admins are forced to their own dealer;
                // SuperAdmin/Admin keep whatever the client actually selected
                // (including empty = "All Dealers"). ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        filter.DealerCode = tokenDealerCode;
                }
                // ─────────────────────────────────────────────────────────────────

                _logger.LogInformation("Job Card Report API Called");
                var result = await _reportService.GetJobReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Dealer Wise Job Card Summary Report</summary>
        [HttpGet("job-card/dealer-wise")]
        [ProducesResponseType(typeof(List<DealerWiseJobReportSummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardDealerWiseReport(
            [FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: token always wins ─────────────────────
                string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                if (!string.IsNullOrEmpty(tokenDealerCode))
                    dealerCode = tokenDealerCode;
                // ─────────────────────────────────────────────────────────────

                var result = await _reportService.GetDealerWiseStockReportAsync(dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer wise stock report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Job Report for specific dealer</summary>
        [HttpGet("job-card/{dealerCode}")]
        [ProducesResponseType(typeof(JobReportPagedResponse<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardReportByDealer
            (string dealerCode, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetJobReportByDealerAsync(
                    dealerCode, pageIndex, pageSize, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching job card report for dealer: {dealerCode}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get filtered Job Card Report</summary>
        [HttpPost("job-card/filter")]
        [ProducesResponseType(typeof(JobReportPagedResponse<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilteredJobCardReport([FromBody] JobReportFilterModel filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _reportService.GetFilteredJobReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("job-card/export")]
        [ProducesResponseType(typeof(List<JobReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportJobCardReport([FromQuery] string? dealerCode,
                    [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        dealerCode = tokenDealerCode;
                }
                // ─────────────────────────────────────────────────────────────────

                var result = await _reportService.GetJobReportForExportAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting job card report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Job Report Summary Statistics</summary>
        [HttpGet("job-card/summary-stats")]
        [ProducesResponseType(typeof(JobReportSummaryStats), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobReportSummaryStats(
            [FromQuery] string dealerCode, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction ────────────────────────────────────────────
                string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                if (!string.IsNullOrEmpty(tokenDealerCode))
                    dealerCode = tokenDealerCode;
                // ─────────────────────────────────────────────────────────────────

                var result = await _reportService.GetReportSummaryStatsAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job report summary stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // VEHICLE SALE
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get Vehicle Sale Report</summary>
        [HttpGet("vehicle-sale")]
        [ProducesResponseType(typeof(List<VehicleSaleReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVehicleSaleReport(
        [FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var result = await _reportService.GetVehicleSaleReportAsync(fromDate, toDate, dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle sale report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get Total Sale Report (Dealer-wise Mapping) — one row per dealer with
        /// full financial totals. Non-admin/dealer users are always restricted
        /// to their own dealer's data, regardless of what dealerCode is passed.
        /// </summary>
        [HttpGet("total-sale-dealer-wise")]
        [ProducesResponseType(typeof(TotalSaleReportDealerWiseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalSaleReportDealerWise(
        [FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: this report exists specifically to show
                // only a dealer's own data, so non-admins can never override it
                // via the dealerCode query param. ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        dealerCode = tokenDealerCode;
                }

                var result = await _reportService.GetTotalSaleReportDealerWiseAsync(fromDate, toDate, dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching total sale report (dealer wise)");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Model Wise Sale Report (Count-wise) — pivoted Dealer x Model</summary>
        [HttpGet("model-wise-sale-count")]
        [ProducesResponseType(typeof(ModelWiseSalePivotResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelWiseSaleCountReport(
        [FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: non-admins always see only their own dealer ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        dealerCode = tokenDealerCode;
                }

                var result = await _reportService.GetModelWiseSaleCountReportAsync(fromDate, toDate, dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model wise sale count report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Model-wise Current Stock Report (Count-wise) — pivoted Dealer x Model</summary>
        [HttpGet("model-wise-stock-count")]
        [ProducesResponseType(typeof(ModelWiseStockPivotResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelWiseStockCountReport(
        [FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: non-admins always see only their own dealer ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        dealerCode = tokenDealerCode;
                }

                var result = await _reportService.GetModelWiseStockCountReportAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model-wise current stock count report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("current-stock")]
        [ProducesResponseType(typeof(PagedResponse<CurrentStockReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentStockReport([FromBody] CurrentStockFilterModel filter)
        {
            try
            {
                if (filter == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Filter model is null"
                    });
                }

                if (filter.PageIndex < 1)
                    filter.PageIndex = 1;

                if (filter.PageSize < 1)
                    filter.PageSize = 20;

                var result = await _reportService
                    .GetCurrentStockReportAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current stock report");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("po-tracking")]
        [ProducesResponseType(typeof(PagedResponse<POTrackingReportViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPOTrackingReport([FromBody] POTrackingFilterModel filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: non-admins always see only their own dealer,
                // overriding whatever DealerCode the client sent in the body ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        filter.DealerCode = tokenDealerCode;
                }

                var result =
                    await _reportService
                    .GetPOTrackingReportAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching PO Tracking Report");

                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }

        [HttpGet("po-tracking/dropdown/po-type")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPOTypeDropdown()
        {
            try
            {
                var result = await _reportService.GetPOTypeDropdownAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PO Type dropdown");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("po-tracking/dropdown/po-status")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPOStatusDropdown()
        {
            try
            {
                var result = await _reportService.GetPOStatusDropdownAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PO Status dropdown");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get Parts Dispatch Report
        /// </summary>
        [HttpGet("parts-dispatch")]
        [ProducesResponseType(typeof(List<PartsDispatchReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartsDispatchReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? dealerCode)
        {
            try
            {
                string userId =
                    GetUserInfoFromToken
                    .GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result =
                    await _reportService
                    .GetPartsDispatchReportAsync(
                        fromDate,
                        toDate,
                        dealerCode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching Parts Dispatch Report");

                return StatusCode(500,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }

        [HttpGet("dealer-list")]
        [ProducesResponseType(typeof(List<PartsDispatchReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDealerList()
        {
            var result =
                await _reportService
                .GetDealerListAsync();

            return Ok(result);
        }

        [HttpGet("model-list/{dealerCode}")]
        [ProducesResponseType(typeof(List<PartsDispatchReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelListByDealer(string dealerCode)
        {
            var result =
                await _reportService
                .GetModelListByDealerAsync(
                    dealerCode);

            return Ok(result);
        }

        [HttpGet("part-dispatch-kit")]
        [ProducesResponseType(typeof(List<PartsDispatchReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartDispatchKitReport([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? dealerCode)
        {
            try
            {
                var result =
                    await _reportService
                    .GetPartDispatchKitReportAsync(
                        fromDate,
                        toDate,
                        dealerCode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching Part Dispatch Kit Report");

                return StatusCode(500,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }

        [HttpGet("part-dispatch-kit-po-types")]
        [ProducesResponseType(typeof(List<PartsDispatchReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartDispatchKitPOTypeDropdown()
        {
            var result =
                await _reportService
                    .GetPartDispatchKitPOTypeDropdownAsync();

            return Ok(result);
        }

        /// <summary>
        /// Get Form 22 report for Vehicle Sale Bill
        /// </summary>
        [HttpGet("Form22")]
        public async Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo)
        {
            try
            {
                return await _reportService.GenerateForm22Report(chassisNo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // VEHICLE SALE BILL REPORT
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost("sale-bill")]
        [ProducesResponseType(typeof(VehicleSaleBillReportPagedResponse), StatusCodes.Status200OK)]
        /// <summary>Get Vehicle Sale Bill Report (paged, with totals)</summary>
        [HttpPost("vehicle-sale-bill")]
        [ProducesResponseType(typeof(VehicleSaleBillReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVehicleSaleBillReport([FromBody] VehicleSaleBillReportFilterModel filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                // ── Dealer restriction: admins/superadmins see all dealers ────
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                // Dealer restriction: non-admins are forced to their own dealer
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        filter.DealerCode = tokenDealerCode;
                }
                // ─────────────────────────────────────────────────────────────

                _logger.LogInformation("Vehicle Sale Bill Report API called");

                var result = await _reportService.GetVehicleSaleBillReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle sale bill report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet("sale-bill/export")]
        [ProducesResponseType(typeof(List<VehicleSaleBillReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportVehicleSaleBillReport([FromQuery] string? dealerCode,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: admins/superadmins see all dealers ────
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        dealerCode = tokenDealerCode;
                }
                // ─────────────────────────────────────────────────────────────

                var result = await _reportService.GetVehicleSaleBillReportForExportAsync(
                    dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting vehicle sale bill report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Sale Type dropdown for the Sale Bill Report filter</summary>
        [HttpGet("sale-bill/dropdown/sale-type")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSaleTypeDropdown()
        {
            try
            {
                var result = await _reportService.GetSaleTypeDropdownAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sale type dropdown");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Status dropdown for the Sale Bill Report filter</summary>
        [HttpGet("sale-bill/dropdown/status")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSaleBillStatusDropdown()
        {
            try
            {
                var result = await _reportService.GetSaleBillStatusDropdownAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sale bill status dropdown");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("print/{id}")]
        public async Task<IActionResult> GetCounterBillPrint(int id)
        {
            try
            {
                var result = await _reportService.GetCounterBillPrintById(id);

                if (result == null)
                    return NotFound("Counter Bill not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // VEHICLE INWARD REPORT
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get Vehicle Inward Report (paged, with totals)</summary>
        [HttpPost("vehicle-inward")]
        [ProducesResponseType(typeof(VehicleInwardReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVehicleInwardReport([FromBody] VehicleInwardReportFilterModel filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                // ── Dealer restriction: non-admins are forced to their own dealer ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        filter.DealerCode = tokenDealerCode;
                }
                // ─────────────────────────────────────────────────────────────────

                _logger.LogInformation("Vehicle Inward Report API called");

                var result = await _reportService.GetVehicleInwardReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle inward report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Get Model-wise Variant Stock Report (Count-wise) — pivoted Model x Colour Variant</summary>
        [HttpGet("model-wise-variant-stock-count")]
        [ProducesResponseType(typeof(ModelWiseVariantStockPivotResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelWiseVariantStockCountReport(
        [FromQuery] string? dealerCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: non-admins always see only their own dealer ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        dealerCode = tokenDealerCode;
                }

                var result = await _reportService.GetModelWiseVariantStockCountReportAsync(dealerCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model-wise variant stock count report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("d2d-report")]
        [ProducesResponseType(typeof(D2DReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetD2DReport([FromBody] D2DReportFilterModel filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 20;

                // ── Dealer restriction: non-admins are forced to their own dealer ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        filter.DealerCode = tokenDealerCode;
                }
                // ─────────────────────────────────────────────────────────────────

                _logger.LogInformation("D2D Report API called");

                var result = await _reportService.GetD2DReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching D2D report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Export D2D Report (unpaged, full dataset matching current filters)</summary>
        [HttpPost("d2d-report/export")]
        [ProducesResponseType(typeof(List<D2DReportViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportD2DReport([FromBody] D2DReportFilterModel filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter model is null" });

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // ── Dealer restriction: non-admins are forced to their own dealer ──
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        filter.DealerCode = tokenDealerCode;
                }
                // ─────────────────────────────────────────────────────────────────

                _logger.LogInformation("D2D Report Export API called");

                var result = await _reportService.GetD2DReportForExportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting D2D report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}