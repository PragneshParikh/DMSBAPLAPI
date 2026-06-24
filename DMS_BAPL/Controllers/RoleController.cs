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

        /// <summary>
        /// Retrieves all roles available in the system.
        /// </summary>
        /// <returns>A list of <see cref="AspNetRole"/> objects representing all system roles.</returns>
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
                if (result.Succeeded) return Ok(new { message = "Role saved and mapped to category." });

                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role with category");
                return StatusCode(500, "An error occurred while saving the role mapping.");
            }
        }

        [HttpGet("by-category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var mappings = await _roleService.GetRolesByCategory(category);
                var roles = mappings.Select(m => new { id = m.RoleId, name = m.RoleName }).ToList();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles by category");
                return StatusCode(500, "An error occurred while fetching roles for the category.");
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
                var data = mappings.Select(m => new { id = m.Id, roleName = m.RoleName, category = m.Category }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching role mappings");
                return StatusCode(500, "An error occurred while fetching role mappings.");
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
    }
}
