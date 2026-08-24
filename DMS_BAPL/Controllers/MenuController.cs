using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.DealerManagerRepo;
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

        // NEW — needed for GetMyAccess below. Same repo DealerManagerController
        // already uses for the identical roleId -> granted-forms lookup.
        private readonly IDealerManagerRepo _dealerManagerRepo;

        public MenuController(IMenuService menuService, ILogger<MenuController> logger, IDealerManagerRepo dealerManagerRepo)
        {
            _menuService = menuService;
            _logger = logger;
            _dealerManagerRepo = dealerManagerRepo;
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

        /// <summary>
        /// NEW — this is the endpoint MenuService.getMyAccess() on the Angular
        /// side has been calling all along (GET {apiUrl}/menu/my-access). It
        /// was 404ing not because of a route mismatch (this controller/route
        /// already existed) but because this specific action never existed on
        /// it. Mirrors DealerManagerController.GetMyAccess(): reads the
        /// "LocationRoleId" claim a Location Login JWT carries, and resolves
        /// that role's granted forms via GetMenuAccessByRoleIdAsync.
        /// </summary>
        [HttpGet("my-access")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyAccess()
        {
            try
            {
                var roleId = User.FindFirst("LocationRoleId")?.Value;

                if (string.IsNullOrWhiteSpace(roleId))
                    return Ok(new { groups = Array.Empty<object>() }); // no location role assigned — empty sidebar, not an error

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