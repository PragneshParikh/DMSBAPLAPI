using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.RoleWiseMenuRightService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/role-wise-menu")]
    [ApiController]
    public class RoleWiseMenuRightController : ControllerBase
    {
        private readonly IRoleWiseMenuRightService _roleWiseMenuRightService;

        public RoleWiseMenuRightController(IRoleWiseMenuRightService roleWiseMenuRightService)
        {
            _roleWiseMenuRightService = roleWiseMenuRightService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RoleWiseMenuRight>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var menuRights = _roleWiseMenuRightService.Get();

                return Ok(menuRights);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        [HttpGet("{RoleId}")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RoleWiseMenuRight>>> GetByRoleId(string roleId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var menuRight = await _roleWiseMenuRightService.GetMenuRightByRoleId(roleId);

                return Ok(menuRight);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }

        }

        [HttpGet("GetByRoleId")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RoleWiseMenuRight>>> GetByRoleId()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var menuRight = await _roleWiseMenuRightService.GetMenuRightByRoleId(null);

                return Ok(menuRight);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }

        }

    }
}