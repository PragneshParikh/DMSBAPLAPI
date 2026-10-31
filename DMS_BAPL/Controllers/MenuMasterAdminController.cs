using DMS_BAPL_Data.Services.MenuMasterAdminService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// Menu Master screen (Admin / SuperAdmin):
    ///   GET    api/menu-master                 all MenuMaster rows (+ role / sub-menu counts)
    ///   POST   api/menu-master                 create menu or sub-menu (optional roles for a sub-menu)
    ///   PUT    api/menu-master/{id}            edit
    ///   DELETE api/menu-master/{id}            delete (only when it has no sub-menus; its rights rows go too)
    ///   GET    api/menu-master/{id}/roles      every role with its permission on this sub-menu
    ///   POST   api/menu-master/{id}/roles      save roles for this sub-menu  [{ roleId, permission }]  (0 = remove)
    /// </summary>
    [Route("api/menu-master")]
    [ApiController]
    public class MenuMasterAdminController : ControllerBase
    {
        private readonly IMenuMasterAdminService _service;
        private readonly ILogger<MenuMasterAdminController> _logger;

        public MenuMasterAdminController(IMenuMasterAdminService service, ILogger<MenuMasterAdminController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private IActionResult? Guard()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Unauthorized(new { success = false, message = "Session expired. Please log in again." });
            var ok = User.Claims.Any(c => c.Type == ClaimTypes.Role &&
                       (c.Value.Equals("superadmin", StringComparison.OrdinalIgnoreCase) || c.Value.Equals("admin", StringComparison.OrdinalIgnoreCase)));
            return ok ? null : StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Only Admin / SuperAdmin can change menus." });
        }

        private async Task<IActionResult> Run(Func<Task<IActionResult>> action, string what)
        {
            var denied = Guard();
            if (denied != null) return denied;
            try { return await action(); }
            catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Menu Master: {What} failed", what);
                return StatusCode(500, new { success = false, message = ex.GetBaseException().Message });
            }
        }

        private string UserId => GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

        [HttpGet]
        public Task<IActionResult> GetAll() =>
            Run(async () => Ok(await _service.GetAllAsync()), "list");

        [HttpPost]
        public Task<IActionResult> Create([FromBody] MenuAdminSaveVM model) =>
            Run(async () => Ok(new { success = true, message = "Menu created.", data = await _service.CreateAsync(model, UserId) }), "create");

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(int id, [FromBody] MenuAdminSaveVM model) =>
            Run(async () => Ok(new { success = true, message = "Menu updated.", data = await _service.UpdateAsync(id, model, UserId) }), "update");

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(int id) =>
            Run(async () => { await _service.DeleteAsync(id); return Ok(new { success = true, message = "Menu deleted." }); }, "delete");

        [HttpGet("{id:int}/roles")]
        public Task<IActionResult> GetRoles(int id) =>
            Run(async () => Ok(await _service.GetRolesAsync(id)), "roles");

        [HttpPost("{id:int}/roles")]
        public Task<IActionResult> SaveRoles(int id, [FromBody] List<MenuAdminRoleVM> roles) =>
            Run(async () => { await _service.SaveRolesAsync(id, roles, UserId); return Ok(new { success = true, message = "Roles saved." }); }, "save roles");
    }
}
