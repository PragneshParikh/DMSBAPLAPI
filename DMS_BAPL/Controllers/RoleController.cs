using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.RoleService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/role")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;
        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AspNetRole>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AspNetRole>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var roles = await _roleService.GetRoles();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching roles");
                throw;
            }
        }

        [HttpPost("with-category")]
        public async Task<IActionResult> CreateWithCategory([FromBody] RoleWithCategoryViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _roleService.CreateRoleWithCategory(model);
                if (!result.Succeeded)
                    return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

                var mappings = await _roleService.GetAllMappings();
                var created = mappings
                    .Where(m => string.Equals(m.Category, model.Category, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(m => m.Id)
                    .FirstOrDefault(m => string.Equals(m.RoleName, model.Name, StringComparison.OrdinalIgnoreCase));
                string? roleId = created?.RoleId;

                return Ok(new { message = "Role saved and mapped to category.", roleId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role with category");
                return StatusCode(500, "An error occurred while saving the role mapping.");
            }
        }

        [HttpGet("mappings")]
        public async Task<IActionResult> GetMappings()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var mappings = await _roleService.GetAllMappings();
                var data = mappings.Select(m => new
                {
                    id = m.Id,
                    roleId = m.RoleId,
                    roleName = m.RoleName,
                    category = m.Category
                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching role mappings");
                return StatusCode(500, "An error occurred while fetching role mappings.");
            }
        }

        [HttpPut("mappings/{id}")]
        public async Task<IActionResult> UpdateMapping(int id, [FromBody] UpdateRoleCategoryViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest(new { message = "Role name is required." });

                var ok = await _roleService.UpdateMapping(id, model.Name.Trim(), model.Category);
                if (!ok) return NotFound(new { message = "Mapping not found." });

                return Ok(new { message = "Role updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role mapping");
                return StatusCode(500, "An error occurred while updating the role mapping.");
            }
        }

        [HttpDelete("mappings/{id}")]
        public async Task<IActionResult> DeleteMapping(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var ok = await _roleService.DeleteMapping(id);
                if (ok) return Ok(new { message = "Mapping removed." });
                return NotFound(new { message = "Mapping not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role mapping");
                return StatusCode(500, "An error occurred while deleting the mapping.");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MENU ACCESS — SALE / SERVICE ROLES ONLY
        // ═══════════════════════════════════════════════════════════════

        [HttpGet("{roleId}/menu-access")]
        public async Task<IActionResult> GetMenuAccess(string roleId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _roleService.GetMenuAccessAsync(roleId);
                if (result == null) return NotFound(new { message = "Role not found under Sale or Service category." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching role menu access");
                return StatusCode(500, "An error occurred while fetching menu access.");
            }
        }

        [HttpPut("{roleId}/menu-access")]
        public async Task<IActionResult> UpdateMenuAccess(string roleId, [FromBody] UpdateRoleMenuAccessViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var (success, error) = await _roleService.UpdateMenuAccessAsync(roleId, model.GrantedSubMenuIds, userId);
                if (!success) return BadRequest(new { message = error });

                return Ok(new { message = "Menu access updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role menu access");
                return StatusCode(500, "An error occurred while updating menu access.");
            }
        }

        // Returns every Process/Reports submenu item, all unchecked — no
        // roleId needed. Powers the Add Role form and Employee Master's
        // per-category checklist.
        [HttpGet("menu-template")]
        public async Task<IActionResult> GetMenuTemplate()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var groups = await _roleService.GetMenuTemplateAsync();
                return Ok(new { groups });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menu template");
                return StatusCode(500, "An error occurred while fetching the menu template.");
            }
        }

        [HttpPost("resolve-for-items")]
        public async Task<IActionResult> ResolveForItems([FromBody] ResolveRoleForItemsViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                if (string.IsNullOrWhiteSpace(model.Category) || model.SubMenuIds == null || model.SubMenuIds.Count == 0)
                    return BadRequest(new { message = "Category and at least one menu item are required." });

                var result = await _roleService.ResolveOrCreateRoleForItemsAsync(model.Category, model.SubMenuIds, userId);
                if (result == null)
                    return BadRequest(new { message = "Could not resolve or create a role for the selected items." });

                return Ok(new { roleId = result.Value.RoleId, roleName = result.Value.RoleName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving role for selected menu items");
                return StatusCode(500, "An error occurred while resolving the role.");
            }
        }
    }
}