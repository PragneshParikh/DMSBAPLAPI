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

        public DealerManagerController(IDealerManagerService service, ILogger<DealerManagerController> logger)
        {
            _service = service;
            _logger = logger;
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
    }
}