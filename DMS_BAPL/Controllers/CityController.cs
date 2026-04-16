using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.CityService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
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

        [HttpGet("CitiesWithStateName")]
        [ProducesResponseType(typeof(IEnumerable<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCitiesWithState()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var cities = await _cityService.GetAllCitiesWithStateAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching cities: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

       
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var city = await _cityService.GetCityByIdAsync(id);

                if (city == null)
                    return NotFound();

                return Ok(city);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching city by id: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

         [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CityCreateEditViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var result = await _cityService.CreateCityAsync(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating city: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

       
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IEnumerable<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] CityCreateEditViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var updated = await _cityService.UpdateCityAsync(id, model);

                if (!updated)
                    return NotFound();

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating city: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("download")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> downloadExcel()
        {
            try
            {

                var file = await _cityService.downloadReceiptExcel();

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "CityMaster.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
