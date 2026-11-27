using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Helpers;
using DMS_BAPL_Data.Repositories.MenuRightsRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// 27.11.2026 - Role Menu Rights screen: rights for ONE BG employee of a role (Sales / Service ...).
    ///   GET    api/menu-rights/employee/list?roleId=        employees mapped to that role (BG Employee Master)
    ///   GET    api/menu-rights/employee/{employeeId}?roleId= menu tree with his rights (his own, or the role's if none)
    ///   POST   api/menu-rights/employee/save                 { employeeId, rights:[{menuId, subMenuId, permission}] }
    ///   DELETE api/menu-rights/employee/{employeeId}         remove his own rights -> he uses the role rights again
    /// Table: dbo.BgEmployeeMenuRight (docs/BgEmployeeMenuRight.sql). Permission bits: View 1, Add 2, Edit 4, Delete 8, Print 16.
    /// </summary>
    [Route("api/menu-rights/employee")]
    [ApiController]
    public class EmployeeMenuRightsController : ControllerBase
    {
        private readonly BapldmsvadContext _context;
        private readonly IMenuRightsRepo _rightsRepo;
        private readonly ILogger<EmployeeMenuRightsController> _logger;

        public EmployeeMenuRightsController(BapldmsvadContext context, IMenuRightsRepo rightsRepo, ILogger<EmployeeMenuRightsController> logger)
        {
            _context = context;
            _rightsRepo = rightsRepo;
            _logger = logger;
        }

        public class SaveEmployeeMenuRightsRequest
        {
            public int EmployeeId { get; set; }
            /// <summary>27.11.2026 - same rights for several employees at once (multi-select on the screen)</summary>
            public List<int>? EmployeeIds { get; set; }
            public List<MenuRightItem> Rights { get; set; } = new();
        }

        private IActionResult? Guard(bool needAdmin)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Unauthorized(new { success = false, message = "Session expired. Please log in again." });
            if (needAdmin && !GetUserInfoFromToken.GetUserGroup(HttpContext))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Only Admin / SuperAdmin can change employee rights." });
            return null;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] string roleId)
        {
            var g = Guard(false); if (g != null) return g;
            try
            {
                var roleName = await _context.AspNetRoles.AsNoTracking()
                    .Where(r => r.Id == roleId).Select(r => r.Name).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(roleName))
                    return Ok(new { tableReady = true, data = Array.Empty<object>() });

                var upper = roleName.Trim().ToUpper();
                var employees = await (
                    from m in _context.BgEmployeeRoleMappings.AsNoTracking()
                    join e in _context.BgEmployeeMasters.AsNoTracking() on m.BgEmployeeId equals e.Id
                    where m.RoleName.ToUpper() == upper
                    select new { e.Id, e.EmployeeCode, e.FirstName, e.LastName, e.EmailId, e.IsActive }
                ).Distinct().ToListAsync();

                // which of them already have their own rights
                var withOwn = new HashSet<int>();
                var tableReady = true;
                try
                {
                    var ids = await _context.Database
                        .SqlQuery<int>($"SELECT DISTINCT BgEmployeeId AS [Value] FROM dbo.BgEmployeeMenuRight WHERE Permission > 0")
                        .ToListAsync();
                    withOwn = ids.ToHashSet();
                }
                catch { tableReady = false; }

                var data = employees
                    .OrderByDescending(e => e.IsActive).ThenBy(e => e.FirstName).ThenBy(e => e.LastName)
                    .Select(e => new
                    {
                        employeeId = e.Id,
                        employeeCode = e.EmployeeCode,
                        name = $"{e.FirstName} {e.LastName}".Trim(),
                        email = e.EmailId,
                        isActive = e.IsActive,
                        hasOwnRights = withOwn.Contains(e.Id)
                    });
                return Ok(new { tableReady, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employee list for role {RoleId} failed", roleId);
                return StatusCode(500, new { success = false, message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("{employeeId:int}")]
        public async Task<IActionResult> Get(int employeeId, [FromQuery] string? roleId)
        {
            var g = Guard(false); if (g != null) return g;
            try
            {
                var tree = await _rightsRepo.GetMenuTreeWithRightsAsync(roleId);
                var own = await BgEmployeeAccess.GetOwnRightsAsync(_context, employeeId);
                var hasOwn = own != null && own.Count > 0;

                if (hasOwn)
                {
                    var lookup = own!.GroupBy(r => r.SubMenuId).ToDictionary(x => x.Key, x => x.Aggregate(0, (a, r) => a | r.Permission));
                    foreach (var sub in tree.SelectMany(t => t.SubMenus))
                        sub.Permission = lookup.TryGetValue(sub.SubMenuId, out var p) ? p : 0;
                }

                return Ok(new { employeeId, hasOwnRights = hasOwn, tableReady = own != null, groups = tree });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employee rights {EmployeeId} failed", employeeId);
                return StatusCode(500, new { success = false, message = ex.GetBaseException().Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SaveEmployeeMenuRightsRequest request)
        {
            var g = Guard(true); if (g != null) return g;
            var empIds = (request?.EmployeeIds ?? new List<int>())
                .Append(request?.EmployeeId ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            if (request == null || empIds.Count == 0)
                return BadRequest(new { success = false, message = "Select at least one employee." });

            var found = await _context.BgEmployeeMasters.Where(e => empIds.Contains(e.Id)).Select(e => e.Id).ToListAsync();
            if (found.Count != empIds.Count) return NotFound(new { success = false, message = "One or more employees were not found." });

            var parentLookup = await _context.MenuMasters.AsNoTracking()
                .Select(m => new { m.Id, m.ParentMenuId })
                .ToDictionaryAsync(m => m.Id, m => m.ParentMenuId ?? m.Id);

            var rows = (request.Rights ?? new List<MenuRightItem>())
                .Where(r => parentLookup.ContainsKey(r.SubMenuId))
                .GroupBy(r => r.SubMenuId)
                .Select(x => x.Last())
                .Select(r => new { r.SubMenuId, MenuId = parentLookup[r.SubMenuId], Permission = r.Permission & 31 })
                .Where(r => r.Permission > 0)
                .ToList();

            var user = GetUserInfoFromToken.GetUserIdFromToken(HttpContext) ?? "admin";
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var empId in empIds)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.BgEmployeeMenuRight WHERE BgEmployeeId = {empId}");
                    foreach (var r in rows)
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync(
                            $"INSERT INTO dbo.BgEmployeeMenuRight (BgEmployeeId, MenuId, SubMenuId, Permission, CreatedBy, CreatedDate) VALUES ({empId}, {r.MenuId}, {r.SubMenuId}, {r.Permission}, {user}, {DateTime.Now})");
                    }
                }
                await tx.CommitAsync();
                var who = empIds.Count == 1 ? "Employee" : $"{empIds.Count} employees'";
                return Ok(new { success = true, message = $"{who} rights saved ({rows.Count} page(s)). They must log in again to see the change." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Save employee rights {EmployeeIds} failed", string.Join(",", empIds));
                var msg = ex.GetBaseException().Message;
                if (msg.Contains("BgEmployeeMenuRight", StringComparison.OrdinalIgnoreCase) && msg.Contains("Invalid object", StringComparison.OrdinalIgnoreCase))
                    msg = "Table BgEmployeeMenuRight is missing - run docs/BgEmployeeMenuRight.sql on the database first.";
                return StatusCode(500, new { success = false, message = "Employee rights not saved: " + msg });
            }
        }

        [HttpDelete("{employeeId:int}")]
        public async Task<IActionResult> ResetToRole(int employeeId)
        {
            var g = Guard(true); if (g != null) return g;
            try
            {
                var n = await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.BgEmployeeMenuRight WHERE BgEmployeeId = {employeeId}");
                return Ok(new { success = true, message = n > 0 ? "Employee now uses the role rights again." : "Employee already uses the role rights." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.GetBaseException().Message });
            }
        }
    }
}
