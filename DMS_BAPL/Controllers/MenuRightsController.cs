using DMS_BAPL_Data.Services.MenuRightsService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/menu-rights")]
    [ApiController]
    public class MenuRightsController : ControllerBase
    {
        private readonly IMenuRightsService _service;

        public MenuRightsController(IMenuRightsService service)
        {
            _service = service;
        }

        [HttpGet("dealers")]
        public async Task<IActionResult> GetDealers()
        {
            try
            {
                var result = await _service.GetDealers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Role-wise Menu Rights: full MenuMaster tree with this role's permissions.</summary>
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetRoleMenuRights(string roleId)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                    return Unauthorized(new { success = false, message = "Session expired. Please log in again." });

                if (string.IsNullOrWhiteSpace(roleId))
                    return BadRequest(new { success = false, message = "RoleId is required." });

                var result = await _service.GetMenuRightsForRole(roleId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Role-wise Menu Rights: save permissions for a role (0 = no access).</summary>
        [HttpPost("role/save")]
        public async Task<IActionResult> SaveRoleMenuRights([FromBody] SaveRoleMenuRightsRequest request)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                    return Unauthorized(new { success = false, message = "Session expired. Please log in again." });

                if (request == null || string.IsNullOrWhiteSpace(request.RoleId))
                    return BadRequest(new { success = false, message = "RoleId is required." });

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                await _service.SaveRoleMenuRights(request, userId);

                return Ok(new { success = true, message = "Menu rights saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{dealerCode}")]
        public async Task<IActionResult> GetMenuRights(string dealerCode)
        {
            try
            {
                var result = await _service.GetMenuRightsForDealer(dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveMenuRights([FromBody] SaveMenuRightsRequest request)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var roleId = await _service.SaveMenuRights(request, userId);

                if (roleId == null)
                    return BadRequest(new { success = false, message = "This dealer has no role assigned. Assign a role first via Dealer Role Assignment Master." });

                return Ok(new { success = true, message = "Menu rights saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}