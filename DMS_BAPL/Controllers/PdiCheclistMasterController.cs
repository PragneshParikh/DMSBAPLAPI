using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.PDIChecklistMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdiCheclistMasterController : ControllerBase
    {
        private readonly IPdiCheckListMaster _pdiCheckListMaster;
        private readonly ILogger<PdiCheclistMasterController> _logger;

        public PdiCheclistMasterController(IPdiCheckListMaster pdiCheckListMaster)
        {
            _pdiCheckListMaster = pdiCheckListMaster;
        }

        [HttpPost("InsertPdiChecklistMaster")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertPdiChecklistMaster(PdiChecklistMasterViemModel pdiChecklistMaster)
        {
            try
            {
                var result = await _pdiCheckListMaster.InsertPdiChecklistMaster(pdiChecklistMaster);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting PDI checklist master.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpPut("UpdatePdiChecklistMaster")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePdiChecklistMaster(PdiChecklistMasterViemModel pdiChecklistMaster)
        {
            try
            {
                var result = await _pdiCheckListMaster.UpdatePdiChecklistMaster(pdiChecklistMaster);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating PDI checklist master.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpDelete("DeletePdiChecklistMaster/{pdicheckId}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePdiChecklistMaster(int pdicheckId)
        {
            try
            {
                var result = await _pdiCheckListMaster.DeletePdiChecklistMaster(pdicheckId);
                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting PDI checklist master.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }


        [HttpGet("GetPdiChecklistMasterList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPdiChecklistMasterList(string? pdicheckName)
        {
            try
            {
                var result = await _pdiCheckListMaster.GetPdiChecklistMasterList(pdicheckName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching PDI checklist master list.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

    }
}
