using DMS_BAPL_Data.Repositories.DealerManagerRepo;
using DMS_BAPL_Data.Services.DealerManagerService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/dealer-creation-manager")]
    [ApiController]
    public class DealerManagerController : ControllerBase
    {
        private readonly IDealerManagerService _service;
        private readonly ILogger<DealerManagerController> _logger;
        private readonly IDealerManagerRepo _dealerManagerRepo;

        public DealerManagerController(IDealerManagerService service, ILogger<DealerManagerController> logger, IDealerManagerRepo dealerManagerRepo)
        {
            _service = service;
            _logger = logger;
            _dealerManagerRepo = dealerManagerRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DealerListFilterModel filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _service.GetAllAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealers");
                return StatusCode(500, "An error occurred while fetching dealers.");
            }
        }

        // CHANGED — accepts `area`, so the Module dropdown reflects only
        // "Area-wise Module" once an Area (ShowRoom/WorkShop/Account) has
        // been chosen. Omitting it returns every top-level module, unchanged.
        [HttpGet("modules")]
        public async Task<IActionResult> GetAvailableModules([FromQuery] string? area)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var modules = await _service.GetAvailableModulesAsync(area);
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available modules");
                return StatusCode(500, "An error occurred while fetching modules.");
            }
        }

        [HttpGet("areas")]
        public async Task<IActionResult> GetAvailableAreas()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var areas = await _service.GetAvailableAreasAsync();
                return Ok(areas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available areas");
                return StatusCode(500, "An error occurred while fetching areas.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound(new { message = "Dealer not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer by id");
                return StatusCode(500, "An error occurred while fetching the dealer.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DealerQuickUpdateViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var (success, error) = await _service.UpdateAsync(id, model);
                if (!success) return BadRequest(new { message = error });

                return Ok(new { message = "Dealer updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dealer");
                return StatusCode(500, "An error occurred while updating the dealer.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var ok = await _service.DeactivateAsync(id);
                if (!ok) return NotFound(new { message = "Dealer not found." });

                return Ok(new { message = "Dealer deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating dealer");
                return StatusCode(500, "An error occurred while deleting the dealer.");
            }
        }

        [HttpPut("{id}/assign-role")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] DealerRoleAssignmentViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var (success, error) = await _service.AssignRoleAsync(id, model.RoleId);
                if (!success) return BadRequest(new { message = error });

                return Ok(new { message = "Role assigned to dealer." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to dealer");
                return StatusCode(500, "An error occurred while assigning the role.");
            }
        }

        [HttpDelete("{id}/assign-role")]
        public async Task<IActionResult> UnassignRole(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var (success, error) = await _service.UnassignRoleAsync(id);
                if (!success) return BadRequest(new { message = error });

                return Ok(new { message = "Role removed from dealer." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from dealer");
                return StatusCode(500, "An error occurred while removing the role.");
            }
        }

        [HttpGet("{id}/menu-access")]
        public async Task<IActionResult> GetMenuAccess(int id, [FromQuery] string? roleId, [FromQuery] string? module, [FromQuery] string? area)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _service.GetMenuAccessAsync(id, roleId, module, area);
                if (result == null) return NotFound(new { message = "Dealer not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer menu access");
                return StatusCode(500, "An error occurred while fetching menu access.");
            }
        }

        [HttpPut("{id}/menu-access")]
        public async Task<IActionResult> UpdateMenuAccess(int id, [FromBody] UpdateDealerMenuAccessViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var (success, error) = await _service.UpdateMenuAccessAsync(id, model.RoleId, model.GrantedSubMenuIds, model.Module, model.Area, userId);
                if (!success) return BadRequest(new { message = error });

                return Ok(new { message = "Menu access updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dealer menu access");
                return StatusCode(500, "An error occurred while updating menu access.");
            }
        }

        [HttpGet("{id}/locations")]
        public async Task<IActionResult> GetLocations(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var result = await _service.GetLocationsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dealer locations");
                return StatusCode(500, "An error occurred while fetching locations.");
            }
        }

        [HttpPut("{id}/locations/bulk-status")]
        public async Task<IActionResult> UpdateLocationsStatus(int id, [FromBody] BulkUpdateLocationStatusViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authorized");

                var (success, error) = await _service.UpdateLocationsStatusAsync(id, model.LocationIds, model.IsActive, userId);
                if (!success) return BadRequest(new { message = error });

                return Ok(new { message = $"{model.LocationIds.Count} location(s) updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location status");
                return StatusCode(500, "An error occurred while updating location status.");
            }
        }

        [HttpGet("my-access")]
        public async Task<IActionResult> GetMyAccess()
        {
            try
            {
                var roleId = User.FindFirst("LocationRoleId")?.Value;

                if (string.IsNullOrWhiteSpace(roleId))
                    return Ok(new { groups = Array.Empty<object>() });
                var full = await _dealerManagerRepo.GetMenuAccessByRoleIdAsync(roleId, module: null, area: null);

                if (full == null)
                    return Ok(new { groups = Array.Empty<object>() });

                var grantedOnly = full.Groups.Select(g => new
                {
                    g.TopMenuId,
                    g.TopMenuName,
                    Items = g.Items.Where(i => i.IsGranted).ToList()
                }).Where(g => g.Items.Count > 0).ToList();

                return Ok(new { roleId, groups = grantedOnly });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching location menu access");
                return StatusCode(500, "An error occurred while fetching your access.");
            }
        }
    }
}