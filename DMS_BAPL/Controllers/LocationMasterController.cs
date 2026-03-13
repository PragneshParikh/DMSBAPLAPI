using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationMasterController : ControllerBase
    {
        private readonly ILocationMasterService _service;

        public LocationMasterController(ILocationMasterService service)
        {
            _service = service;
        }
        [HttpGet("GetAllLocationMaster")]
        public async Task<IActionResult> GetAllLocationMaster()
        {
            var result = await _service.GetAllLocationMaster();
            return Ok(result);
        }
        [HttpGet("GetLocationMasterById/{id}")]
        public async Task<IActionResult> GetLocationMasterById(int id)
        {
            var result = await _service.GetLocationMasterById(id);
            return Ok(result);
        }
        [HttpPost("AddLocationMaster")]
        public async Task<IActionResult> AddLocationMaster(LocationMasterViewModel model)
        {
            var result = await _service.AddLocationMaster(model);
            return Ok(result);
        }
        [HttpPut("UpdateLocationMaster")]
        public async Task<IActionResult> UpdateLocationMaster(LocationMasterViewModel model)
        {
            var result = await _service.UpdateLocationMaster(model);
            return Ok(result);
        }
        [HttpGet("DownloadLocationMasterExcel")]
        public async Task<IActionResult> DownloadLocationMasterExcel()
        {
            try
            {
                var file = await _service.DownloadLocationMasterExcel();

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
    }
}
