using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.FreeServiceClaimService;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ClientModel.Primitives;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/free-service-claim")]
    [ApiController]
    public class FreeServiceClaimController : ControllerBase
    {
        private readonly IFreeServiceClaimService _freeServiceClaimService;
        private readonly IPrefixService _prefixService;

        public FreeServiceClaimController(IFreeServiceClaimService freeServiceClaimService, IPrefixService prefixService)
        {
            _freeServiceClaimService = freeServiceClaimService;
            _prefixService = prefixService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(FreeServiceClaimHeader), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var serviceClaim = await _freeServiceClaimService.Get();

                return Ok(serviceClaim);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetPendingApprovalJobCard")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetPendingApprovalJobCard([FromQuery] string? dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var freeClaims = await _freeServiceClaimService.GetPendingApprovalJobCard(dealerCode);

                return Ok(freeClaims);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetWarrantyClaimByDealerCode")]
        [ProducesResponseType(typeof(FreeServiceClaimHeaderViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetWarrantyClaimByDealerCode(
            [FromQuery] string dealerCode,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageIndex = 1)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var claims = await _freeServiceClaimService.GetWarrantyClaimByDealerCode(dealerCode, pageSize, pageIndex);

                return Ok(claims);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(FreeServiceClaimViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetClaimById(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var claims = await _freeServiceClaimService.GetClaimById(Id);

                return Ok(claims);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Insert([FromBody] FreeServiceClaimViewModel freeServiceClaimViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var freeClaim = await _freeServiceClaimService.Insert(freeServiceClaimViewModel);

                if (freeClaim)
                {
                    await _prefixService.UpdateNextNumberByDealerByModule(freeServiceClaimViewModel.DealerCode, "free_service_claim");
                }

                return Ok(freeClaim);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update([FromBody] FreeServiceClaimViewModel freeServiceClaimViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var claims = await _freeServiceClaimService.Update(freeServiceClaimViewModel);

                return Ok(claims);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
