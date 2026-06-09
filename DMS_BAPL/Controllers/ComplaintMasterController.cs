using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.ComplaintMasterRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintMasterController : ControllerBase
    {
        private readonly IComplaintMaster _complaintMaster;
        private readonly ILogger<ComplaintMasterController> _logger;

        public ComplaintMasterController(IComplaintMaster complaintMaster, ILogger<ComplaintMasterController> logger)
        {
            _complaintMaster = complaintMaster;
            _logger = logger;
        }

        [HttpGet("GetComplaintMasterList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComplaintMasterList()
        {
            try
            {
                var result = await _complaintMaster.GetComplaintMasterList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetComplaintMasterList");
                return StatusCode(500, "An error occurred while fetching complaint master list.");
            }
        }

        [HttpGet("GetComplaintMasterById/{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComplaintMasterById(int complaintId)
        {
            try
            {
                var result = await _complaintMaster.GetComplaintMasterById(complaintId);
                if (result == null)
                    return NotFound("Complaint master not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetComplaintMasterById");
                return StatusCode(500, "An error occurred while fetching complaint master details.");
            }

        }

        [HttpPost("InsertComplaintMaster")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> InsertComplaintMaster(ComplaintMasterViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _complaintMaster.InsertComplaintMaster(model, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertComplaintMaster");
                return StatusCode(500, "An error occurred while inserting complaint master.");
            }
        }
        [HttpPut("UpdateComplaintMaster")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateComplaintMaster(ComplaintMasterViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _complaintMaster.UpdateComplaintMaster(model, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateComplaintMaster");
                return StatusCode(500, "An error occurred while updating complaint master.");
            }
        }

        [HttpDelete("DeleteComplaintMaster/{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteComplaintMaster(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _complaintMaster.DeleteComplaintMaster(id, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteComplaintMaster");
                return StatusCode(500, "An error occurred while deleting complaint master.");
            }
        }

        [HttpGet("GetComplaintMasterExcel")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComplaintMasterExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _complaintMaster.DownloadComplaintMasterExcel();
                return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ComplaintMaster.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetComplaintMasterExcel");
                return StatusCode(500, "An error occurred while downloading complaint master Excel.");
            }
        }
    }
}