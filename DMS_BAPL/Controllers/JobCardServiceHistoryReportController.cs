using DMS_BAPL_Data.Services.JobCardServiceHistoryImportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// Reports on the imported service history (BAPLDMSvad.dbo.JobCardServiceHistory). Read-only. Admin / SuperAdmin only.
    ///   GET api/jobcard-servicehistory-report/service-history?search=&amp;page=&amp;pageSize=            Chassis No / Reg No
    ///   GET api/jobcard-servicehistory-report/service-history/export?search=                       .xlsx (all 63 columns)
    ///   GET api/jobcard-servicehistory-report/job-data?dealerCode=&amp;fromDate=&amp;toDate=&amp;page=&amp;pageSize=
    ///   GET api/jobcard-servicehistory-report/job-data/export?dealerCode=&amp;fromDate=&amp;toDate=          .xlsx
    ///   GET api/jobcard-servicehistory-report/dealers                                              dealer dropdown
    /// </summary>
    [Route("api/jobcard-servicehistory-report")]
    [ApiController]
    public class JobCardServiceHistoryReportController : ControllerBase
    {
        private const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private readonly IJobCardServiceHistoryReportService _report;
        private readonly ILogger<JobCardServiceHistoryReportController> _logger;

        public JobCardServiceHistoryReportController(IJobCardServiceHistoryReportService report, ILogger<JobCardServiceHistoryReportController> logger)
        {
            _report = report;
            _logger = logger;
        }

        private IActionResult? Guard()
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Only Admin / SuperAdmin can view service history reports." });
            return null;
        }

        private async Task<IActionResult> Run(Func<Task<IActionResult>> action, string what)
        {
            var denied = Guard();
            if (denied != null) return denied;
            try { return await action(); }
            catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
            catch (OperationCanceledException) { return StatusCode(499); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service history report {What} failed", what);
                return StatusCode(500, new { success = false, message = $"{what} failed: " + ex.GetBaseException().Message });
            }
        }

        [HttpGet("service-history")]
        public Task<IActionResult> ServiceHistory([FromQuery] JcshServiceHistoryQuery q, CancellationToken ct) =>
            Run(async () => Ok(await _report.ServiceHistoryAsync(q, ct)), "Service History search");

        /// <summary>type-ahead: GET service-history/suggest?term=p6 → up to 10 chassis + 10 reg numbers starting with it</summary>
        [HttpGet("service-history/suggest")]
        public Task<IActionResult> Suggest([FromQuery] string? term, CancellationToken ct) =>
            Run(async () => Ok(await _report.SuggestAsync(term, ct)), "Suggestions");

        /// <summary>Vehicle History Card (JC-Print format): GET service-history/card?search=&amp;exact=</summary>
        [HttpGet("service-history/card")]
        public Task<IActionResult> HistoryCard([FromQuery] JcshServiceHistoryQuery q, CancellationToken ct) =>
            Run(async () => Ok(await _report.HistoryCardAsync(q, ct)), "History Card");

        [HttpGet("job-data")]
        public Task<IActionResult> JobData([FromQuery] JcshJobDataQuery q, CancellationToken ct) =>
            Run(async () => Ok(await _report.JobDataAsync(q, ct)), "Job Data");

        [HttpGet("dealers")]
        public Task<IActionResult> Dealers(CancellationToken ct) =>
            Run(async () => Ok(await _report.DealersAsync(ct)), "Dealer list");

        [HttpGet("service-history/export")]
        public Task<IActionResult> ExportServiceHistory([FromQuery] JcshServiceHistoryQuery q, CancellationToken ct) =>
            Run(() => Export(s => _report.ExportServiceHistoryAsync(q, s, ct),
                $"Service_History_{SafeName(q.Search)}_{DateTime.Now:yyyyMMdd}.xlsx"), "Service History export");

        /// <summary>letters / digits only for the download file name</summary>
        private static string SafeName(string? s)
        {
            var t = new string((s ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            return t.Length == 0 ? "Search" : t.Length > 30 ? t[..30] : t;
        }

        [HttpGet("job-data/export")]
        public Task<IActionResult> ExportJobData([FromQuery] JcshJobDataQuery q, CancellationToken ct) =>
            Run(() => Export(s => _report.ExportJobDataAsync(q, s, ct),
                $"Job_Data_{(string.IsNullOrWhiteSpace(q.DealerCode) ? "AllDealers" : q.DealerCode.Trim())}_{q.FromDate:yyyyMMdd}-{q.ToDate:yyyyMMdd}.xlsx"), "Job Data export");

        /// <summary>Builds the .xlsx in a temp file (deleted when the response is sent) and returns it.</summary>
        private async Task<IActionResult> Export(Func<Stream, Task<int>> write, string fileName)
        {
            var path = Path.Combine(Path.GetTempPath(), $"dms-jcsh-report-{Guid.NewGuid():N}.xlsx");
            var fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 1 << 16, FileOptions.DeleteOnClose);
            try
            {
                var rows = await write(fs);
                fs.Position = 0;
                Response.Headers["X-Row-Count"] = rows.ToString();
                Response.Headers["Access-Control-Expose-Headers"] = "Content-Disposition, X-Row-Count";
                return File(fs, XlsxMime, fileName);   // MVC disposes the stream after sending → temp file deleted
            }
            catch
            {
                await fs.DisposeAsync();
                throw;
            }
        }
    }
}
