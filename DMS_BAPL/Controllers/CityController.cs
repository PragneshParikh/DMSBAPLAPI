using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.CityService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/city")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;
        private readonly ILogger<CityController> _logger;

        public CityController(ICityService cityService, ILogger<CityController> logger)
        {
            _cityService = cityService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<City>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var cities = await _cityService.GetCitiesAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving cities : {ex.Message}");
                throw;
            }
        }
    }
}
