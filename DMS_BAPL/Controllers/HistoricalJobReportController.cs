using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/historical-job-report")]
    [ApiController]
    public class HistoricalJobReportController : ControllerBase
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;
        private readonly ILogger<HistoricalJobReportController> _logger;

        public HistoricalJobReportController(
            BapldmsvadContext context,
            IExcelService excelService,
            ILogger<HistoricalJobReportController> logger)
        {
            _context = context;
            _excelService = excelService;
            _logger = logger;
        }

        private bool TryApplyDealerScope(HistoricalJobReportFilter filter)
        {
            bool isSuperAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);

            if (isSuperAdmin)
                return true;

            var dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

            if (string.IsNullOrWhiteSpace(dealerCode))
                return false;

            filter.DealerCode = dealerCode;
            return true;
        }

        private IQueryable<DmsJobReport> BuildQuery(HistoricalJobReportFilter filter)
        {
            var query = _context.DmsJobReports.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.DealerCode == filter.DealerCode);

            if (filter.FromDate.HasValue)
            {
                var from = DateOnly.FromDateTime(filter.FromDate.Value.Date);
                query = query.Where(x => x.JobDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = DateOnly.FromDateTime(filter.ToDate.Value.Date);
                query = query.Where(x => x.JobDate <= to);
            }

            return query;
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<object>>> List([FromQuery] HistoricalJobReportFilter filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (!TryApplyDealerScope(filter))
                    return Unauthorized("Dealer code not found for user");

                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 10;
                if (filter.PageSize > 100) filter.PageSize = 100;

                var query = BuildQuery(filter);

                int totalRecords = await query.CountAsync();

                var result = await query
                    .OrderBy(x => x.JobDate)
                    .ThenBy(x => x.Id)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return Ok(new PagedResponse<object>
                {
                    Data = result.Cast<object>().ToList(),
                    TotalRecords = totalRecords
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching historical job report");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.GetBaseException().Message });
            }
        }

        private static string Fmt(DateOnly? d) => d.HasValue ? d.Value.ToString("dd-MM-yyyy") : "";

        private static readonly (string Header, Func<DmsJobReport, object?> Get)[] ExportColumns =
        {
            ("Dealer Name",         x => x.DealerName),
            ("Dealer Code",         x => x.DealerCode),
            ("Dealer Location",     x => x.DealerLocation),
            ("City",                x => x.City),
            ("State",               x => x.State),
            ("Job No",              x => x.JobNo),
            ("Job Date",            x => Fmt(x.JobDate)),
            ("Job End Date",        x => Fmt(x.JobEndDate)),
            ("Job Type",            x => x.JobType),
            ("Service Head",        x => x.ServiceHead),
            ("Service Type",        x => x.ServiceType),
            ("Job Status",          x => x.JobStatus),
            ("Kms",                 x => x.Kms),
            ("Customer Name",       x => x.CustomerName),
            ("Mobile No",           x => x.MobileNo),
            ("Chassis No",          x => x.ChassisNo),
            ("Reg No",              x => x.RegNo),
            ("Engine No",           x => x.EngineNo),
            ("Brand",               x => x.BrandName),
            ("Item Name",           x => x.ItemName),
            ("Customer Voice",      x => x.CustomerVoice),
            ("Complaint Code",      x => x.ComplaintCode),
            ("Observation",         x => x.Observation),
            ("Supervisor Comment",  x => x.SupervisorComment),
            ("Battery No",          x => x.BatteryNo),
            ("Charger No",          x => x.ChargerNo),
            ("Sale Date",           x => Fmt(x.SaleDate)),
            ("Supervisor",          x => x.Supervisor),
            ("Technician",          x => x.Technician),
            ("Created Through",     x => x.CreatedThrough),
            ("Action",              x => x.Action),
        };

        [HttpGet("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Download([FromQuery] HistoricalJobReportFilter filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (!TryApplyDealerScope(filter))
                    return Unauthorized("Dealer code not found for user");

                var query = BuildQuery(filter);

                int total = await query.CountAsync();
                if (total > 100000)
                    return BadRequest(new { message = $"Too many rows ({total}). Please narrow the date range (max 1,00,000)." });

                var data = await query
                    .OrderBy(x => x.JobDate)
                    .ThenBy(x => x.Id)
                    .ToListAsync();

                var columns = new List<string> { "Sr No" };
                columns.AddRange(ExportColumns.Select(c => c.Header));

                var rows = new List<Dictionary<string, object>>();
                int sr = 1;
                foreach (var item in data)
                {
                    var dict = new Dictionary<string, object> { ["Sr No"] = sr++ };
                    foreach (var col in ExportColumns)
                        dict[col.Header] = col.Get(item) ?? "";
                    rows.Add(dict);
                }

                var model = new ExcelExportViewModel
                {
                    SheetName = "Job Report",
                    Columns = columns,
                    Rows = rows
                };

                var file = await _excelService.GenerateExcel(model);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "HistoricalJobReport.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting historical job report");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.GetBaseException().Message });
            }
        }
    }

    public class HistoricalJobReportFilter
    {
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}