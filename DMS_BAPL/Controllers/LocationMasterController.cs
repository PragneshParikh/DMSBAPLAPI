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

        public LocationMasterController(ILocationMasterService locationMasterService, IDealerMasterService dealerMasterService)
        {
            _dealerService = dealerMasterService;
            _locationMasterService = locationMasterService;
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
            var result = await _locationMasterService.GetLocationMasterById(id);
            return Ok(result);
        }
        [HttpPost("AddLocationMaster")]
        public async Task<IActionResult> AddLocationMaster(LocationMasterViewModel model)
        {
            var result = await _locationMasterService.AddLocationMaster(model);
            return Ok(result);
        }
        [HttpPut("UpdateLocationMaster")]
        public async Task<IActionResult> UpdateLocationMaster(LocationMasterViewModel model)
        {
            var result = await _locationMasterService.UpdateLocationMaster(model);
            return Ok(result);
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
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("GetAllShowroomLocationsofCurrentDelaer")]
        public async Task<IActionResult> GetAllShowroomlocation(string dealerCode)
        {
            var result = await _locationMasterService.GetLocationByDealerCode(dealerCode);
            return Ok(result);

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
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("GetLocationTypeWiseNameByDealerCode")]
        [ProducesResponseType(typeof(IEnumerable<LocationNameViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocationTypeWiseNameByDealerCode(string dealerCode)
        {
            try
            {
                var data = await _locationMasterService.GetLocationNameTypewiseListAsync(dealerCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpdateByLocationCode/{locCode}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> UpdateByLocationCode(string locCode, LocationMasterViewModel locationMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var location = await _locationMasterService.UpdateByLocationCode(locCode, userId, locationMasterViewModel);

                return Ok(location);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
