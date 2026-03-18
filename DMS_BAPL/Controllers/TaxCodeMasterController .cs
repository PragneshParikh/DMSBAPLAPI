using DMS_BAPL_Data.Services.TaxCodeMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxCodeMasterController : ControllerBase
    {
        private readonly ITaxCodeMasterService _taxCodeMasterService;

        public TaxCodeMasterController(ITaxCodeMasterService taxCodeMasterService)
        {
            _taxCodeMasterService = taxCodeMasterService;
        }
        [HttpGet("GetAllTaxCodes")]
        public async Task<IActionResult> GetAllTaxCodes()
        {
            var data = await _taxCodeMasterService.GetAllTaxCodes();
            return Ok(data);
        }

        [HttpGet("GetTaxCodeById/{id}")]
        public async Task<IActionResult> GetTaxCodeById(int id)
        {
            var data = await _taxCodeMasterService.GetTaxCodeById(id);

            if (data == null)
            {
                return NotFound("Record not found.");
            }

            return Ok(data);
        }
        //[Authorize]
        [HttpPost("AddTaxCode")]
        public async Task<IActionResult> AddTaxCode([FromBody] TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            if (taxCodeMasterViewModel == null)
            {
                return BadRequest("Invalid data.");
            }

            var data = await _taxCodeMasterService.AddTaxCode(taxCodeMasterViewModel);

            return Ok(new
            {
                Message = "Tax code added successfully.",
                Id = data
            });
        }

        [HttpPut("UpdateTaxCode")]
        public async Task<IActionResult> UpdateTaxCode([FromBody] TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            if (taxCodeMasterViewModel == null || taxCodeMasterViewModel.Id <= 0)
            {
                return BadRequest("Invalid data.");
            }

            var data = await _taxCodeMasterService.UpdateTaxCode(taxCodeMasterViewModel);

            if (data == 0)
            {
                return NotFound("Record not found.");
            }

            return Ok(new
            {
                Message = "Tax code updated successfully."
            });
        }
        [HttpGet("DownloadTaxCodeExcel")]
        public async Task<IActionResult> DownloadTaxCodeExcel()
        {
            var fileData = await _taxCodeMasterService.DownloadTaxCodeExcel();

            return File(fileData,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "TaxCodeMaster.xlsx");
        }
    }

}
