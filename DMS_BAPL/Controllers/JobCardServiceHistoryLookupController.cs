using DMS_BAPL_Data.Services.JobCardServiceHistoryImportService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// Job card creation (Android app): look a vehicle up in the imported service history
    /// (dbo.DMS_ServiceHistory - see JcshTarget; was BAPLDMSvad.dbo.JobCardServiceHistory until 23.11.2026) by Chassis No or
    /// Registration No - typed or read by OCR - and return its details to pre-fill the job card.
    ///
    /// Added 18.11.2026, dealer scope added 19.11.2026. Additive only: no existing endpoint, service or table changed.
    /// Any logged-in DMS user may call it (dealers create job cards; the report endpoints are Admin / SuperAdmin only).
    ///   GET api/jobcard-servicehistory-lookup/suggest?term=P6DEC&amp;scope=dealer|all
    ///       up to 15 chassis + 15 reg numbers starting with the term (no customer data)
    ///   GET api/jobcard-servicehistory-lookup/vehicle?value=...&amp;scope=dealer|all
    ///       one picked value (exact match on ChassisNo / RegNo): vehicle(s) with their jobs, newest first
    /// scope=dealer (default): only jobs of the logged-in dealer - the dealer code is taken from the JWT, never from
    /// the request. Admin / SuperAdmin have no dealer, so they always get all dealers. scope=all: every dealer.
    /// Read-only; all SQL parameterised.
    /// </summary>
    [Route("api/jobcard-servicehistory-lookup")]
    [ApiController]
    public class JobCardServiceHistoryLookupController : ControllerBase
    {
        private const string Table = JcshTarget.TableName;
        private const int SuggestPerKind = 15;

        private readonly IJobCardServiceHistoryReportService _report;
        private readonly IConfiguration _config;
        private readonly ILogger<JobCardServiceHistoryLookupController> _logger;

        public JobCardServiceHistoryLookupController(IJobCardServiceHistoryReportService report, IConfiguration config,
            ILogger<JobCardServiceHistoryLookupController> logger)
        {
            _report = report;
            _config = config;
            _logger = logger;
        }

        /// <summary>dealer code to filter on, or null for all dealers</summary>
        private string? DealerFilter(string? scope)
        {
            if (string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase)) return null;
            if (GetUserInfoFromToken.GetUserGroup(HttpContext)) return null;          // Admin / SuperAdmin
            var dealer = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
            return string.IsNullOrWhiteSpace(dealer) ? null : dealer.Trim();
        }

        private async Task<IActionResult> Run(Func<Task<IActionResult>> action, string what)
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            try { return await action(); }
            catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
            catch (OperationCanceledException) { return StatusCode(499); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job card service history lookup {What} failed", what);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"{what} failed: " + ex.GetBaseException().Message });
            }
        }

        [HttpGet("suggest")]
        public Task<IActionResult> Suggest([FromQuery] string? term, [FromQuery] string? scope, CancellationToken ct) =>
            Run(async () =>
            {
                var dealer = DealerFilter(scope);
                if (dealer == null) return Ok(await _report.SuggestAsync(term, ct));
                return Ok(await DealerSuggestAsync(term, dealer, ct));
            }, "Service history suggestions");

        [HttpGet("vehicle")]
        public Task<IActionResult> Vehicle([FromQuery] string? value, [FromQuery] string? scope, CancellationToken ct) =>
            Run(async () =>
            {
                var v = (value ?? "").Trim();
                if (v.Length < 3) return BadRequest(new { success = false, message = "Enter a Chassis No or Registration No." });
                var card = await _report.HistoryCardAsync(new JcshServiceHistoryQuery { Search = v, Exact = true }, ct);
                var dealer = DealerFilter(scope);
                if (dealer == null) return Ok(card);

                // dealer scope: only this dealer's jobs; vehicles with none of them are left out
                var vehicles = new List<JcshCardVehicle>();
                foreach (var veh in card.Vehicles)
                {
                    var jobs = veh.Jobs.Where(j => j.TryGetValue("DealerCode", out var d) &&
                        string.Equals(Convert.ToString(d)?.Trim(), dealer, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (jobs.Count == 0) continue;
                    veh.Jobs = jobs;
                    vehicles.Add(veh);
                }
                return Ok(new JcshHistoryCard
                {
                    Vehicles = vehicles,
                    TotalJobs = vehicles.Sum(x => x.Jobs.Count),
                    Truncated = card.Truncated,
                    Limit = card.Limit
                });
            }, "Service history vehicle lookup");

        /// <summary>
        /// SuggestAsync of the report service, limited to one dealer's jobs (uses the ChassisNo / RegNo indexes,
        /// then the dealer filter). Same output shape (JcshSuggestion).
        /// </summary>
        private async Task<List<JcshSuggestion>> DealerSuggestAsync(string? term, string dealer, CancellationToken ct)
        {
            var s = new string((term ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            if (s.Length < 2) return new List<JcshSuggestion>();
            if (s.Length > 30) s = s[..30];

            var cs = JcshTarget.ConnectionString(_config);
            if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException($"{JcshTarget.ConnectionName} / {JcshTarget.FallbackConnectionName} is not configured on the API.");

            var result = new List<JcshSuggestion>();
            await using var con = new SqlConnection(cs);
            await con.OpenAsync(ct);
            foreach (var (col, other, kind) in new[] { ("ChassisNo", "RegNo", "Chassis No"), ("RegNo", "ChassisNo", "Reg No") })
            {
                var isReg = col == "RegNo";
                var values = new List<string>();
                await using (var cmd = new SqlCommand(
                    $"SELECT TOP (@n) {col} FROM {Table} WHERE {col} LIKE @s + '%' AND DealerCode = @d GROUP BY {col} ORDER BY {col};", con)
                    { CommandTimeout = 30 })
                {
                    cmd.Parameters.Add("@n", SqlDbType.Int).Value = isReg ? SuggestPerKind * 4 : SuggestPerKind;
                    cmd.Parameters.Add("@s", SqlDbType.NVarChar, 30).Value = s;
                    cmd.Parameters.Add("@d", SqlDbType.NVarChar, 20).Value = dealer;
                    await using var rd = await cmd.ExecuteReaderAsync(ct);
                    while (await rd.ReadAsync(ct))
                    {
                        var v = rd.GetString(0);
                        if (!isReg || JobCardServiceHistoryReportService.IsValidRegNo(v, null)) values.Add(v);
                    }
                }
                values = values.Take(SuggestPerKind).ToList();
                if (values.Count == 0) continue;

                var rows = new List<(string Val, string? Other, string? Model, int Jobs, DateTime? Last)>();
                var names = values.Select((_, i) => "@v" + i).ToList();
                await using (var cmd = new SqlCommand(
                    $"SELECT {col}, {other}, MAX(Model), COUNT(*), MAX(JobDate) FROM {Table} " +
                    $"WHERE DealerCode = @d AND {col} IN ({string.Join(", ", names)}) GROUP BY {col}, {other};", con) { CommandTimeout = 30 })
                {
                    cmd.Parameters.Add("@d", SqlDbType.NVarChar, 20).Value = dealer;
                    for (var i = 0; i < values.Count; i++) cmd.Parameters.Add(names[i], SqlDbType.NVarChar, 30).Value = values[i];
                    await using var rd = await cmd.ExecuteReaderAsync(ct);
                    while (await rd.ReadAsync(ct))
                        rows.Add((rd.GetString(0), rd.IsDBNull(1) ? null : rd.GetString(1), rd.IsDBNull(2) ? null : rd.GetString(2),
                                  rd.GetInt32(3), rd.IsDBNull(4) ? null : rd.GetDateTime(4)));
                }

                foreach (var g in rows.GroupBy(r => r.Val).OrderBy(g => g.Key, StringComparer.Ordinal))
                {
                    var latest = g.OrderByDescending(r => r.Last ?? DateTime.MinValue).First();
                    // chassis hit → its most used real Reg No; reg hit → its most used chassis no
                    var o = g.Where(r => !string.IsNullOrWhiteSpace(r.Other) &&
                                         (isReg || JobCardServiceHistoryReportService.IsValidRegNo(r.Other, g.Key)))
                             .OrderByDescending(r => r.Jobs).ThenByDescending(r => r.Last ?? DateTime.MinValue)
                             .Select(r => r.Other!.Trim()).FirstOrDefault();
                    result.Add(new JcshSuggestion
                    {
                        Value = g.Key,
                        Kind = kind,
                        ChassisNo = isReg ? o : g.Key,
                        RegNo = isReg ? g.Key : o,
                        Model = latest.Model ?? g.Select(r => r.Model).FirstOrDefault(m => m != null),
                        DealerCode = dealer,
                        Jobs = g.Sum(r => r.Jobs),
                        LastJobDate = latest.Last
                    });
                }
            }
            return result;
        }
    }
}
