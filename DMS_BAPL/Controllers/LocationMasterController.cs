using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationMasterController : ControllerBase
    {
        private readonly IDealerMasterService _dealerService;
        private readonly ILocationMasterService _locationMasterService;
        private readonly ILogger<LocationMasterController> _logger;

        public LocationMasterController(ILocationMasterService locationMasterService, IDealerMasterService dealerMasterService, ILogger<LocationMasterController> logger)
        {
            _dealerService = dealerMasterService;
            _locationMasterService = locationMasterService;
            _logger = logger;
        }

        [HttpGet("GetAllLocationMaster")]
        public async Task<IActionResult> GetAllLocationMaster()
        {
            var result = await _locationMasterService.GetAllLocationMaster();
            return Ok(result);
        }

        [HttpGet("GetLocationMasterById/{id}")]
        public async Task<IActionResult> GetLocationMasterById(int id)
        {
            try
            {
                var result = await _locationMasterService.GetLocationMasterById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("AddLocationMaster")]
        public async Task<IActionResult> AddLocationMaster(LocationMasterViewModel model)
        {
            try
            {
                var result = await _locationMasterService.AddLocationMaster(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut("UpdateLocationMaster")]
        public async Task<IActionResult> UpdateLocationMaster(LocationMasterViewModel model)
        {
            try
            {
                var result = await _locationMasterService.UpdateLocationMaster(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("DownloadLocationMasterExcel")]
        public async Task<IActionResult> DownloadLocationMasterExcel()
        {
            try
            {
                var file = await _locationMasterService.DownloadLocationMasterExcel();

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "LocationMaster.xlsx"
                );
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetAllShowroomLocationsofCurrentDelaer")]
        public async Task<IActionResult> GetAllShowroomlocation(string dealerCode)
        {
            try
            {
                var result = await _locationMasterService.GetLocationByDealerCode(dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet("GetLocationByDealerCode/{dealerCode}")]
        [ProducesResponseType(typeof(IEnumerable<LocationNameViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocationByDealerCode(string dealerCode)
        {
            try
            {
                var data = await _locationMasterService.GetLocationByDealerCode(dealerCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetLocationDropdownByDealerCode")]
        [ProducesResponseType(typeof(IEnumerable<LocationNameViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocationDropdownByDealerCode([FromQuery] string? dealerCode = null)
        {
            try
            {
                var data = await _locationMasterService.GetLocationDropdownByDealerCode(dealerCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetLocationTypeWiseNameByDealerCode")]
        [ProducesResponseType(typeof(IEnumerable<LocationNameViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocationTypeWiseNameByDealerCode(string? dealerCode)
        {
            try
            {
                var data = await _locationMasterService.GetLocationNameTypewiseListAsync(dealerCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetLocationByDealerByAreaId")]
        [ProducesResponseType(typeof(IEnumerable<LocationNameViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LocationNameViewModel>> GetLocationByDealerByAreaId([FromQuery] string? dealerCode, [FromQuery] int areaId)
        {
            try
            {
                if (dealerCode == "null")
                {
                    dealerCode = null;
                }

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var locations = await _locationMasterService.GetLocationByDealerByAreaId(dealerCode, areaId);

                return Ok(locations);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Insert/Update data from the ERP
        /// </summary>
        /// <param name="locationMasterViewModel"></param>
        /// <returns></returns>
        [HttpPut("UpdateByLocationCode")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> UpdateByLocationCode([FromBody] LocationMasterViewModel locationMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var (location, isNew) = await _locationMasterService.UpdateByLocationCode(userId, locationMasterViewModel);

                if (location == null)
                    return NotFound("Location not found");

                return Ok(new { data = location, isNew });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Imports location master data from an uploaded Excel (.xlsx) file.
        /// Rows are matched by Location Code: an existing location is updated, a new
        /// Location Code is inserted. The sheet's first row must be a header row —
        /// column order doesn't matter as long as the header text matches what
        /// LocationMasterService.ImportLocationExcelAsync expects.
        /// </summary>
        [HttpPost("ImportLocationMasterExcel")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(LocationImportResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportLocationMasterExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                var extension = Path.GetExtension(file.FileName);
                if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only .xlsx files are supported.");

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _locationMasterService.ImportLocationExcelAsync(file, userId);

                return Ok(new
                {
                    message = "Location data imported successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetDealerPrimaryLocationByAreaId")]
        [ProducesResponseType(typeof(IEnumerable<LocationMasterViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetDealerPrimaryLocationByAreaId([FromQuery] int areaId, [FromQuery] string locCode, [FromQuery] string? dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var locations = await _locationMasterService.GetDealerPrimaryLocationByAreaId(areaId, locCode, dealerCode);

                return Ok(locations);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet("GetAllLocationByDealerCode/{dealerCode}")]
        public async Task<IActionResult> GetAllLocationByDealerCode(string dealerCode)
        {
            try
            {
                var data = await _locationMasterService.GetAllLocationByDealerCode(dealerCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }



    }
}