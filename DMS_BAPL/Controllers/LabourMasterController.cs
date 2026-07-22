using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.LabourMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabourMasterController : ControllerBase
    {

        private readonly ILabourMasterRepo _labourMasterRepo;

        public LabourMasterController(ILabourMasterRepo labourMasterRepo)
        {
            _labourMasterRepo = labourMasterRepo;
        }
        [HttpPost("ImportModelWiseLabourMasterExcelApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportModelWiseLabourMasterExcelApi([FromForm] LabourMasterViewModel labourMasterViewModel)
        {
            try
            {
                string? createdBy = User.Identity?.Name ?? "Admin";

                var result = await _labourMasterRepo.ImportLabourExcel(labourMasterViewModel, createdBy);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    LabourMasterData = new List<object>()
                });
            }
        }

        [HttpPost("ImportPartWiseLabourMasterExcelApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportPartWiseLabourMasterExcelApi([FromForm] LabourMasterViewModel labourMasterViewModel)
        {
            try
            {
                string? createdBy = User.Identity?.Name ?? "Admin";
                var result = await _labourMasterRepo.ImportPartWiseLabourExcel(labourMasterViewModel, createdBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    labourPartWiseData = new List<object>()
                });
            }
        }

        [HttpPut("UpdateLabourMasterDataApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLabourMasterDataApi([FromBody] LabourMasteUpdateViewModel labourMasterUpdateViewModel)
        {
            try
            {
                string? updatedBy = User.Identity?.Name ?? "Admin";
                var result = await _labourMasterRepo.UpdateLabourMaster(labourMasterUpdateViewModel, updatedBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    labourPartWiseData = new List<object>()
                });
            }
        }

        [HttpPut("UpdatePartWiseLabourMasterDataApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePartWiseLabourMasterDataApi([FromBody] PartWiseLabourMasterRateViewModel partWiseLabourMasterRateViewModel)
        {
            try
            {
                string? updatedBy = User.Identity?.Name ?? "Admin";
                var result = await _labourMasterRepo.UpdatePartWiseLabourMaster(partWiseLabourMasterRateViewModel, updatedBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    labourPartWiseData = new List<object>()
                });
            }
        }

        [HttpGet("GetLabourMasterModelwiseListApi")]
        [ProducesResponseType(typeof(PagedResponse<List<LabourMasteUpdateViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLabourMasterModelwiseListApi(string? searchText)
        {
            try
            {
                var result = await _labourMasterRepo.GetLabourMasterModelwiseList(searchText);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    Data = new List<LabourMasteUpdateViewModel>()
                });
            }
        }
        [HttpGet("GetLabourMasterPartwiseListApi")]
        [ProducesResponseType(typeof(PagedResponse<List<PartWiseLabourMasterRateViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLabourMasterPartwiseListApi(string? searchText)
        {
            try
            {
                var result = await _labourMasterRepo.GetLabourMasterPartwiseList(searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    Data = new List<PartWiseLabourMasterRateViewModel>()
                });
            }
        }

        [HttpGet("GetLabourRateDropDown/{oemmodelName}/{customerLedgerId}/{dealerCode}")]
        [ProducesResponseType(typeof(PagedResponse<List<PartWiseLabourMasterRateViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLabourRateDropDown(string oemmodelName,int customerLedgerId,string dealerCode)
        {
            try
            {
                var result = await _labourMasterRepo.GetLabourRateDropDowns(oemmodelName, customerLedgerId,dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    Data = new List<LabourRateDropDown>()
                });
            }
        }

    }
}
