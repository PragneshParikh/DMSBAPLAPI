using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.FFIRRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FFIRController : ControllerBase
    {
        private readonly IFFIRRepo _ffirRepo;

        public FFIRController(IFFIRRepo ffirRepo)
        {
            _ffirRepo = ffirRepo;
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
            var jobCardHistory = await _ffirRepo.GetJobCardHistory(chassisNo);
            return Ok(jobCardHistory);
        }

        [HttpPost("InsertFFIR")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertFFIR([FromBody] FFIRViewModel model)
        {
            try
            {
                var result = await _ffirRepo.InsertFFIRAsync(model);

                return Ok(new
                {
                    success = true,
                    message = StringConstants.FFIRInsert,
                    id = result
                });
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
