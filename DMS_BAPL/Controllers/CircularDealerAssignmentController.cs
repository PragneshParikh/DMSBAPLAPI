using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.CircularDealerAssignmentService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/circular-dealer-assignment")]
    [ApiController]
    public class CircularDealerAssignmentController : ControllerBase
    {
        private readonly ICircularDealerAssignmentService _circularDealerAssignmentService;
        private readonly ILogger<CircularDealerAssignmentController> _logger;

        public CircularDealerAssignmentController(ICircularDealerAssignmentService circularDealerAssignmentService, ILogger<CircularDealerAssignmentController> logger)
        {
            _circularDealerAssignmentService = circularDealerAssignmentService;
            _logger = logger;
        }

        [HttpGet("{circularId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAssignmentByCircularId(int circularId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var assignments = await _circularDealerAssignmentService.GetAssignmentByCircularId(circularId);

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featcching assignment by CircularId : {ex.Message}");
                throw;
            }
        }

        [HttpPost("{circularId}/dealer-permissions")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AssignDealersToCircular(int circularId, [FromBody] List<CircularDealerAssignmentViewModel> circularDealerAssignment)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var circularAssignment = await _circularDealerAssignmentService.AssignDealersToCircular(circularId, circularDealerAssignment);

                return Ok(circularAssignment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while assigning dealers to circular: {ex.Message}");
                throw;
            }
        }

        [HttpDelete("{circularId}/dealer-permissions")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteDealersCircularPermission(int circularId, [FromBody] List<CircularDealerAssignmentViewModel> circularDealerAssignment)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var circularAssignment = await _circularDealerAssignmentService.DeleteDealersCircularPermission(circularId, circularDealerAssignment);

                return Ok(circularAssignment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while assigning dealers to circular: {ex.Message}");
                throw;
            }
        }
    }
}
