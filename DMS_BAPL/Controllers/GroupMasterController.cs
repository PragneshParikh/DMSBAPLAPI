using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.GroupMasterRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupMasterController : ControllerBase
    {
        private readonly IGroupMasterRepo _groupMasterRepo;
        private readonly ILogger<GroupMasterController> _logger;

        public GroupMasterController(IGroupMasterRepo groupMasterRepo, ILogger<GroupMasterController> logger)
        {
            _groupMasterRepo = groupMasterRepo;
            _logger = logger;
        }

        [HttpGet("GetAllGroups")]
        [ProducesResponseType(typeof(List<GroupMasterViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllGroups()
        {
            try
            {
                var result = await _groupMasterRepo.GetAllGroups();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllGroups");
                return StatusCode(500, "An error occurred while retrieving groups.");
            }
        }

        [HttpPost("InsertGroup")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertGroup(GroupMasterViewModel groupMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _groupMasterRepo.InsertGroup(groupMasterViewModel, userId);
                if (result == -1)
                    return StatusCode(500, "An error occurred while inserting the group.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertGroup");
                return StatusCode(500, "An error occurred while inserting the group.");
            }
        }
        [HttpPut("UpdateGroupName")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateGroupName([FromBody] GroupMasterViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _groupMasterRepo.UpdateGroupName(model, userId);
                if (result == -1)
                    return NotFound($"Group with ID {model.Id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateGroupName");
                return StatusCode(500, "An error occurred while updating the group name.");
            }
        }

        [HttpDelete("DeleteGroup/{groupId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            try
            {
                var result = await _groupMasterRepo.DeleteGroup(groupId);
                if (result == -1)
                    return NotFound($"Group with ID {groupId} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteGroup");
                return StatusCode(500, "An error occurred while deleting the group.");
            }
        }

        [HttpGet("GetGroupMasterExcel")]
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
                var result = await _groupMasterRepo.DownloadGroupMasterExcel();
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
