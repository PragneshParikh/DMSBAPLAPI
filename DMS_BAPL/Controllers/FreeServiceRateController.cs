using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.FreeServiceRateService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/free-service-rate")]
    [ApiController]
    public class FreeServiceRateController : ControllerBase
    {
        private readonly IFreeServiceRateService _freeServiceRateService;

        public FreeServiceRateController(IFreeServiceRateService freeServiceRateService)
        {
            _freeServiceRateService = freeServiceRateService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FreeServiceRate>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var freeServiceRate = await _freeServiceRateService.Get();

                return Ok(freeServiceRate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetByOEMMOdelId")]
        [ProducesResponseType(typeof(IEnumerable<FreeServiceRate>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetByOEMModelId([FromQuery] int? OEMModelId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var freeServiceRate = await _freeServiceRateService.GetByOEMModelId(OEMModelId);

                return Ok(freeServiceRate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Insert(List<FreeServiceRateViewModel> freeServiceRateViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var freeServiceRate = await _freeServiceRateService.Insert(freeServiceRateViewModel);

                return Ok(freeServiceRate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(FreeServiceRateViewModel freeServiceViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var freeServiceRate = await _freeServiceRateService.Update(freeServiceViewModel);

                return Ok(freeServiceRate);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
