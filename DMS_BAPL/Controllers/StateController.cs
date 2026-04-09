using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.StateService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/state")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly IStateService _stateService;
        private readonly ILogger<StateController> _logger;

        public StateController(IStateService stateService, ILogger<StateController> logger)
        {
            _stateService = stateService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<State>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<State>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var states = await _stateService.GetStatesAsync();

                return Ok(states);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving states : {ex.Message}");
                throw;
            }
        }
    }
}
