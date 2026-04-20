using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.MenuMasterService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/menu")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuService menuService, ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all menu items available in the system for the authenticated user.
        /// </summary>
        /// <returns>A list of <see cref="MenuMasterViewModel"/> objects representing the menu items.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MenuMasterViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MenuMasterViewModel>>> GetMenuItems()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var menuItems = await _menuService.GetMenuItems();

                return Ok(menuItems);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching menu items");
                throw;
            }
        }
    }
}
