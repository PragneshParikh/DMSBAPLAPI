using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Data.Repositories.WarrantyJobCardClaimRepo;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyJCClaimController : ControllerBase
    {
        private readonly IWarrantyJobCardClaimRepo _WarrantyjobCardClaimRepo;
        private readonly ILogger<WarrantyJCClaimController> _logger;
        private readonly IPrefixService _prefixService;

        public WarrantyJCClaimController(IWarrantyJobCardClaimRepo warrantyJobCardClaimRepo,
            ILogger<WarrantyJCClaimController> logger, IPrefixService prefixService)
        {

            _WarrantyjobCardClaimRepo = warrantyJobCardClaimRepo;
            _logger = logger;
            _prefixService = prefixService;
        }


        [HttpPost("InsertWarrantyJCClaim")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertWarrantyJCClaim(WarrantyJCClaimViewModel warrantyJCClaimViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");



                var result = await _WarrantyjobCardClaimRepo.InsertWarrantyJCClaim(warrantyJCClaimViewModel, userId);
                if (result > 0)
                {
                    await _prefixService.UpdateNextNumberByDealerByModule(warrantyJCClaimViewModel.DealerCode, "wclaim_prefix");
                    return Ok(new
                    {
                        message = StringConstants.WarrantyDetailsSaved
                    });
                }
                else
                {
                    _logger.LogError("Failed to insert JobCard.");
                    return StatusCode(500, "An error occurred while saving job card details.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveJobCardDetails");
                return StatusCode(500, "An error occurred while saving job card details.");
            }
        }



    }
}
