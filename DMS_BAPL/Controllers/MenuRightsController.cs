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