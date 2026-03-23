using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.BatteryCapacityMasterService;
using DMS_BAPL_Utils;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatteryCapacityMasterController : ControllerBase
    {
        private readonly IBatteryCapacityMasterService _batteryCapacityMasterService;

        public BatteryCapacityMasterController(IBatteryCapacityMasterService batteryCapacityMasterService)
        {
            _batteryCapacityMasterService = batteryCapacityMasterService;

        }
        /// <summary>
        /// Creates a new battery capacity record.
        /// </summary>
        /// <param name="batteryCapacityMasterViewModel">Battery capacity data</param>
        /// <returns>Created record</returns>

        [HttpPost("create")]
        public async Task<IActionResult> CreateBatterCapacityMaster([FromBody] BatteryCapacityMasterViewModel batteryCapacityMasterViewModel)
        {
            try
            {
                if (batteryCapacityMasterViewModel == null)
                    return BadRequest(StringConstants.BadRequest);
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest("User not found");
                var result = await _batteryCapacityMasterService.AddBatteryCapacityMasterAsync(batteryCapacityMasterViewModel, userId);

                return Ok(new
                {
                    message = StringConstants.BatteryCapacityMasterCreated,
                    data = result
                });
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

        /// <summary>
        /// Retrieves all battery capacity records.
        /// </summary>
        /// <returns>List of battery capacity records</returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetAllBatteryCapacityMasterAsync()
        {
            try
            {
                var batteryCapacityMasters = await _batteryCapacityMasterService.GetBatteryCapacityMastersAsync();

                return Ok(new
                {
                    message = StringConstants.DealerFetched,
                    data = batteryCapacityMasters ?? new List<BatteryCapacityMaster>()
                });
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

        /// <summary>
        /// Updates an existing battery capacity record.
        /// </summary>
        /// <param name="id">Record ID</param>
        /// <param name="batteryCapacityMasterViewModel">Updated data</param>
        /// <returns>Updated record</returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBatteryCapacityMasterAsync(int id, [FromBody] BatteryCapacityMasterViewModel batteryCapacityMasterViewModel)
        {
            try
            {
                if (batteryCapacityMasterViewModel == null)
                    return BadRequest(StringConstants.BadRequest);
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest("User not found");
                var result = await _batteryCapacityMasterService.UpdateBatteryCapacityMasterAsync(id, batteryCapacityMasterViewModel, userId);

                if (result == null)
                {
                    return NotFound(StringConstants.BatteryCapacityMasterUpdateFailed);
                }

                return Ok(new
                {
                    message = StringConstants.BatteryCapacityMasterUpdated,
                    data = result
                });
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

        /// <summary>
        /// Retrieves paginated battery capacity records with optional filtering.
        /// </summary>
        /// <param name="batteryCapacity">Filter by battery capacity</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <returns>Paginated result set</returns>

        [HttpGet("listPaginated")]
        public async Task<IActionResult> GetPaginatedResultAsync(string? batteryCapacity, int? page = null, int? pageSize = null)
        {
            try
            {
                var result = await _batteryCapacityMasterService
                    .GetPaginatedBatteryCapacityMastersAsync(batteryCapacity, page, pageSize);

                if (result == null || result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        message = "No records found",
                        totalRecords = 0,
                        data = new List<BatteryCapacityMaster>()
                    });
                }

                return Ok(new
                {
                    message = "Battery Capacity fetched successfully",
                    totalRecords = result.TotalRecords,
                    data = result.Data
                });
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
        /// <summary>
        /// Downloads battery capacity data as an Excel file.
        /// </summary>
        /// <returns>Excel file</returns>
        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {
            try
            {

            var file = await _batteryCapacityMasterService.DownloadDealerExcel();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Battery Capacity Master List.xlsx"
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