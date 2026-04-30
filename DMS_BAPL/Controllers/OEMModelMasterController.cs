using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.OEMModelMasterRepo;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Data.Services.OEMModelMasterService;
using DMS_BAPL_Data.ViewModels;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OEMModelMasterController : ControllerBase
    {
        private readonly IOEMModelMasterService _oemmasterservice;
        public OEMModelMasterController(IOEMModelMasterService oemmasterservice)
        {
            _oemmasterservice = oemmasterservice;
        }

        [HttpGet("GetAllOEMModels")]
        public async Task<IActionResult> GetAllOEMModels()
        {
            var data = await _oemmasterservice.GetAllOEMModels();
            return Ok(data);
        }

        [HttpGet("GetOEMModelByStatus/{isActive}")]
        [ProducesResponseType(typeof(IEnumerable<OemmodelMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OemmodelMaster>>> GetOEMModelByStatus(Boolean isActive)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var oemModels = await _oemmasterservice.GetOEMModelByStatus(isActive);

                return Ok(oemModels);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetOEMModelById/{id}")]
        public async Task<IActionResult> GetOEMModelById(int id)
        {
            var data = await _oemmasterservice.GetOEMModelById(id);
            return Ok(data);
        }

        [HttpPost("AddOEMModel")]
        public async Task<IActionResult> AddOEMModel(OEMModelMasterViewModel oemViewModel)
        {
            var result = await _oemmasterservice.AddOEMModel(oemViewModel);
            return Ok(result);
        }

        [HttpDelete("DeleteOEMModel/{id}")]
        public async Task<IActionResult> DeleteOEMModel(int id)
        {
            var result = await _oemmasterservice.DeleteOEMModel(id);
            return Ok(result);
        }
        [HttpPut("UpdateOEMModel")]
        public async Task<IActionResult> UpdateOEMModel(OEMModelMasterViewModel oemViewModel)
        {
            var result = await _oemmasterservice.UpdateOEMModel(oemViewModel);

            if (!result)
                return NotFound("Record not found");

            return Ok("OEM Model Updated Successfully");
        }
        [HttpGet("DownloadOEMModelExcel")]
        public async Task<IActionResult> DownloadOEMModelExcel()
        {
            var fileBytes = await _oemmasterservice.DownloadOEMModelExcel();

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "OEMModelMaster.xlsx"
            );
        }
    }
}
