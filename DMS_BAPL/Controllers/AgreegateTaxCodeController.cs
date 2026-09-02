using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.AgreetaxcodeService;
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
    public class AgreegateTaxCodeController : ControllerBase
    {
        private readonly IAgreegateTaxcodeService _aggregateTaxCodeService;

        public AgreegateTaxCodeController(IAgreegateTaxcodeService aggregateTaxCodeService)
        {
            _aggregateTaxCodeService = aggregateTaxCodeService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertAggregateTaxCode([FromBody] AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        {
            try
            {
                var result = await _aggregateTaxCodeService.InsertAgreeTaxcodeAsync(agreeTaxCodeViewModel);
                return Ok(new
                {
                    Message = StringConstants.AggregateTaxCodeCreated,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAggregateTaxCodes(string? search)
        {
            try
            {
                var result = await _aggregateTaxCodeService.GetAggregateTaxcodesAsync(search);
                return Ok(new
                {
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("details/{ataxCode}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAggregateTaxCodesByAtaxCode(string ataxCode)
        {
            try
            {
                var result = await _aggregateTaxCodeService.GetAggregateTaxDetailsAsync(ataxCode);
                if (result == null)
                {
                    return NotFound($"Aggregate Tax Code with AtaxCode {ataxCode} not found.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateAggregateTaxCode(int id , [FromBody] AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        //{
        //    var result = await _aggregateTaxCodeService.UpdateAgreeTaxcodeAsync(id, agreeTaxCodeViewModel);

        //    if (result == null)
        //    {
        //        return NotFound($"Aggregate Tax Code with ID {agreeTaxCodeViewModel.Id} not found.");
        //    }
        //    return Ok(new
        //    {
        //        Message = StringConstants.AggregateTaxCodeUpdated,
        //        Data = result
        //    });
        //}

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAggregateTaxCodeById(int id)
        {
            try
            {
                var result = await _aggregateTaxCodeService.GetAggregateTaxcodeByIdAsync(id);
                if (result == null)
                {
                    return NotFound($"Aggregate Tax Code with ID {id} not found.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpGet("taxcodes-with-rate")]
        public async Task<IActionResult> GetTaxCodesWithRate()
        {
            try
            {
                var result = await _aggregateTaxCodeService.GetTaxCodeWithRate();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Imports Aggregate Tax Code rows from an uploaded Excel (.xlsx) file as an upsert:
        /// a row matching an existing (AtaxCode, TaxCode) pair is updated, everything else
        /// is inserted. Existing data is never deleted. A row missing AtaxCode or TaxCode
        /// (this table requires a valid TaxCode/TaxRate, checked against Tax Code Master, on
        /// every entry) is skipped and reported rather than failing the whole import.
        /// </summary>
        /// <param name="file">Excel file (.xlsx) containing Aggregate Tax Code rows</param>
        /// <returns>Import summary: counts of inserted/updated/failed rows, plus any row-level error messages</returns>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AggregateTaxImportResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportAggregateTaxCodes(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                var extension = Path.GetExtension(file.FileName);
                if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only .xlsx files are supported.");

                var result = await _aggregateTaxCodeService.ImportAggregateTaxCodeExcelAsync(file);

                return Ok(new
                {
                    Message = "Aggregate Tax Code data imported successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}