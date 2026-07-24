using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.BgRoleService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/bg-role")]
    [ApiController]
    public class BgRoleController : ControllerBase
    {
        private readonly IBgRoleService _bgRoleService;
        private readonly ILogger<BgRoleController> _logger;

        public BgRoleController(IBgRoleService bgRoleService, ILogger<BgRoleController> logger)
        {
            _bgRoleService = bgRoleService;
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

                var roles = await _bgRoleService.GetRoles();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching BG roles");
                throw;
            }
        }

        [HttpPost("with-category")]
        public async Task<IActionResult> CreateWithCategory([FromBody] BgRoleWithCategoryViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _bgRoleService.CreateRoleWithCategory(model, userId);   // now passes userId
                if (result.Succeeded) return Ok(new { message = "BG Role saved and mapped to category." });

                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating BG role with category");
                return StatusCode(500, "An error occurred while saving the BG role mapping.");
            }
        }

        [HttpGet("by-category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var mappings = await _bgRoleService.GetRolesByCategory(category);
                var roles = mappings.Select(m => new { id = m.RoleId, name = m.RoleName }).ToList();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching BG roles by category");
                return StatusCode(500, "An error occurred while fetching BG roles for the category.");
            }
        }

        [HttpGet("mappings")]
        public async Task<IActionResult> GetMappings()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var mappings = await _bgRoleService.GetAllMappings();
                var data = mappings.Select(m => new { id = m.Id, roleName = m.RoleName, category = m.Category }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching BG role mappings");
                return StatusCode(500, "An error occurred while fetching BG role mappings.");
            }
        }

        [HttpPut("mappings/{id}")]
        public async Task<IActionResult> UpdateMapping(int id, [FromBody] UpdateBgRoleCategoryViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _bgRoleService.UpdateMapping(id, model.Name, model.Category);
                if (result.Succeeded) return Ok(new { message = "BG Role updated." });

                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating BG role mapping");
                return StatusCode(500, "An error occurred while updating the BG role mapping.");
            }
        }
        [HttpDelete("mappings/{id}")]
        public async Task<IActionResult> DeleteMapping(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var ok = await _bgRoleService.DeleteMapping(id);
                if (ok) return Ok(new { message = "BG Role mapping removed." });
                return NotFound(new { message = "Mapping not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting BG role mapping");
                return StatusCode(500, "An error occurred while deleting the mapping.");
            }
        }
    }
}