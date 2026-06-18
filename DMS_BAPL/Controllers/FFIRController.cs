using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.FFIRRepo;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FFIRController : ControllerBase
    {
        private readonly IFFIRRepo _ffirRepo;
        private readonly IPrefixService _prefixService;
        private readonly ILogger<FFIRController> _logger;

        public FFIRController(IFFIRRepo ffirRepo, IPrefixService prefixService, ILogger<FFIRController> logger)
        {
            _ffirRepo = ffirRepo;
            _prefixService = prefixService;
            _logger = logger;
        }

        [HttpGet("GetPartDropdownlist")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartDropdownlist()
        {
            var partDropdownlist = await _ffirRepo.GetPartDropdownlist();
            return Ok(partDropdownlist);
        }
        [HttpGet("GetComplaintCodeList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComplaintCodeList()
        {
            var complaintCodeList = await _ffirRepo.GetComplaintCodeList();
            return Ok(complaintCodeList);
        }

        [HttpGet("GetJobCardHistory/{chassisNo}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardHistory(string chassisNo)
        {
            try
            {
                var jobCardHistory = await _ffirRepo.GetJobCardHistory(chassisNo);
                return Ok(jobCardHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPost("InsertFFIR")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertFFIR([FromBody] FFIRViewModel model)
        {
            try
            {
                
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _ffirRepo.InsertFFIRAsync(model,userId);
                if (result > 0)
                {
                    await _prefixService.UpdateNextNumberByDealerByModule(model.DealerCode, "ffir_prefix");
                    return Ok(new
                    {
                        success = true,
                        message = StringConstants.FFIRInsert,
                        id = result
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
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("GetFFIRDetailListing")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFFIRDetailListing(string dealerCode, string? search)
        {
            var ffirDetailListing = await _ffirRepo.GetFFIRDetailListing(dealerCode, search);
            return Ok(ffirDetailListing);

        }

        [HttpGet("GetFFIRById/{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFFIRById(int id)
        {
            var result = await _ffirRepo.GetFFIRById(id);

            return Ok(result);
        }

        [HttpPut("UpdateFFIR/{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFFIR(int id, [FromBody] FFIRViewModel model)
        {
            try
            {
                var result = await _ffirRepo.UpdateFFIRAsync(id, model);

                if (!result)
                    return NotFound();

                return Ok(new
                {
                    message = "FFIR Updated Successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
