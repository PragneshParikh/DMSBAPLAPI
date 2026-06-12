using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.TermConditionMasterRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermConditionController : ControllerBase
    {
        private readonly ILogger<TermConditionController> _logger;
        private readonly ITermConditionMasterRepo _termConditionMasterRepo;

        public TermConditionController(ILogger<TermConditionController> logger, ITermConditionMasterRepo termConditionMasterRepo)
        {
            _logger = logger;
            _termConditionMasterRepo = termConditionMasterRepo;
        }
        [HttpPost("AddTermCondition")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddTermCondition(TermConditionMasterViewModel conditionMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _termConditionMasterRepo.AddTermCondition(conditionMasterViewModel, userId);
                if (result == -1)
                    return StatusCode(500, "An error occurred while adding the term condition.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding term condition.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPut("UpdateTermCondition")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTermCondition(TermConditionMasterViewModel conditionMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _termConditionMasterRepo.UpdateTermCondition(conditionMasterViewModel, userId);
                if (result == -1)
                    return StatusCode(500, "An error occurred while updating the term condition.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating term condition.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("DeleteTermCondition/{conditionId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTermCondition(int conditionId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _termConditionMasterRepo.DeleteTermCondition(conditionId);
                if (result == -1)
                    return StatusCode(500, "An error occurred while deleting the term condition.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting term condition.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GetAllTermConditions")]
        [ProducesResponseType(typeof(List<TermConditionMasterViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTermConditions()
        {
            try
            {
                var result = await _termConditionMasterRepo.GetAllTermCondition();
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGroupMasterExcel");
                return StatusCode(500, "An error occurred while downloading group master Excel.");
            }
        }

        [HttpGet("GetTermConditionMasterExcel")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGroupMasterExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _termConditionMasterRepo.DownloadTermConditionMasterExcel();
                return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GroupMaster.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGroupMasterExcel");
                return StatusCode(500, "An error occurred while downloading group master Excel.");
            }
        }
    }
}
