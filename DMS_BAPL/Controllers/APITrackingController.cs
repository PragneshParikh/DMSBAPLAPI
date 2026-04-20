using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.APITracking;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Services.APITrackingService;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Utils.Helpers;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/api-tracking")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class APITrackingController : ControllerBase
    {
        private readonly IAPITrackingService _apiTrackingService;
        private readonly ILogger<APITrackingController> _logger;

        public APITrackingController(IAPITrackingService apiTrackingService, ILogger<APITrackingController> logger)
        {
            _apiTrackingService = apiTrackingService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves API tracking records from the system.
        /// Validates the user from the token before fetching the data.
        /// </summary>
        /// <returns>
        /// 200 OK with a list of API tracking records if successful,
        /// 401 Unauthorized if the user is not authenticated,
        /// 500 Internal Server Error if an exception occurs.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Apitracking>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAPITracking()
        {

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var apiTracking = await _apiTrackingService.GetAPITracking();

                return Ok(apiTracking);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching API tracking records");
                throw;
            }
        }

        /// <summary>
        /// Retrieves filtered API tracking records based on the specified criteria.
        /// Validates the user from the token before fetching the data.
        /// </summary>
        /// <param name="fromDate">Start date for filtering records.</param>
        /// <param name="ToDate">End date for filtering records.</param>
        /// <param name="endPoint">Optional endpoint to filter by API path.</param>
        /// <param name="searchCriteria">Optional search keyword to filter results.</param>
        /// <param name="status">Optional status to filter records.</param>
        /// <returns>
        /// 200 OK with a list of filtered API tracking records if successful,
        /// 401 Unauthorized if the user is not authenticated,
        /// 500 Internal Server Error if an exception occurs.
        /// </returns>
        [HttpGet("FilterData")]
        [ProducesResponseType(typeof(IEnumerable<Apitracking>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetFilterData([FromQuery] DateTime fromDate, [FromQuery] DateTime ToDate, [FromQuery] string endPoint = "", [FromQuery] string searchCriteria = "", [FromQuery] string status = "")
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var apitrackings = await _apiTrackingService.GetFilterRecords(fromDate, ToDate, endPoint, searchCriteria, status);

                return Ok(apitrackings);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching filtered API tracking records");
                throw;
            }
        }

        /// <summary>
        /// Generates and returns an Excel file containing API tracking data.
        /// Validates the user from the token before generating the file.
        /// </summary>
        /// <returns>
        /// 200 OK with the Excel file if successful,
        /// 401 Unauthorized if the user is not authenticated,
        /// 500 Internal Server Error if an exception occurs.
        /// </returns>
        [HttpGet("DownloadExcel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadOEMModelExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var fileBytes = await _apiTrackingService.DownloadAPIExcel();

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "APIExcel.xlsx"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while downloading API tracking Excel");
                throw;
            }
        }
    }
}
