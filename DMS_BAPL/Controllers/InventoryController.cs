using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    public class PartInventoryController : ControllerBase
    {
        private readonly IPartInventoryService _partInventoryService;

        public PartInventoryController(IPartInventoryService partInventoryService)
        {
            _partInventoryService = partInventoryService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var inventory = await _partInventoryService.Get();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetByItemCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByItemCode(List<string> itemCodes)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var inventory = await _partInventoryService.GetByItemCode(itemCodes);

                return Ok(inventory);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetPartsByDealerAndDateRange")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartsByDealerAndDateRange([FromQuery] InventoryFilterViewModel inventoryFilterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authotized");

                var inventory = await _partInventoryService.GetPartsByDealerAndDateRange(inventoryFilterViewModel);

                return Ok(inventory);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
