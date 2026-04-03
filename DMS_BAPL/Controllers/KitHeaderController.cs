using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.KitHeaderService;
using DMS_BAPL_Utils.Helpers;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/kit-header")]
    [ApiController]
    public class KitHeaderController : ControllerBase
    {
        private readonly IKitHeaderService _kitHeaderService;
        private readonly ILogger<KitHeaderController> _logger;

        public KitHeaderController(IKitHeaderService kitHeaderService, ILogger<KitHeaderController> logger)
        {
            _kitHeaderService = kitHeaderService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(KitHeader), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<KitHeader?>> GetKitById(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var kitHeader = await _kitHeaderService.GetKitById(id);

                return Ok(kitHeader);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something wrong when featching kit by id : {ex.Message}");
                throw;
            }
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponse<KitHeader>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<KitHeader>>> GetKitHeaderByPagedAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var kit = await _kitHeaderService.GetKitByPagedAsync(searchTerm, pageIndex, pageSize);

                return Ok(kit);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error: {ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> SaveKitHeader(KitHeader kitHeader)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var _kit = await _kitHeaderService.InsertKitHeader(kitHeader);

                return Ok(_kit);
            }
            catch (Exception ex)
            {
                _logger.LogError($"kit header not saved : {ex.Message}");
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateKitHeader(KitHeader kitHeader)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var _kit = await _kitHeaderService.UpdateKitHeader(kitHeader);

                return Ok(_kit);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in updating kit header : {ex.Message}");
                throw;
            }
        }
    }
}
