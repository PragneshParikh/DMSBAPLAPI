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
        private readonly ILogger<RoleWiseMenuRightController> _logger;

        public RoleWiseMenuRightController(IRoleWiseMenuRightService roleWiseMenuRightService, ILogger<RoleWiseMenuRightController> logger)
        {
            _roleWiseMenuRightService = roleWiseMenuRightService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all role-wise menu rights for the authenticated user.
        /// </summary>
        /// <returns>A list of role-wise menu rights.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RoleWiseMenuRight>>> Get()
        {
            string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var menuRights = await _roleWiseMenuRightService.Get();

                return Ok(menuRights);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching role-wise menu rights");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all menu rights assigned to a specific role.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role whose menu rights are to be fetched.</param>
        /// <returns>A list of <see cref="RoleWiseMenuRight"/> objects corresponding to the specified role.</returns>
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
                _logger.LogError("Error occurred while fetching menu rights");
                throw;
            }

        }

        /// <summary>
        /// Retrieves all role-wise menu rights. 
        /// If no roleId is specified, it returns menu rights for all roles.
        /// </summary>
        /// <returns>A list of <see cref="RoleWiseMenuRight"/> objects representing the menu rights for each role.</returns>
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
                _logger.LogError(ex, "Error occurred while fetching role-wise menu rights");
                throw;
            }

        }

    }
}