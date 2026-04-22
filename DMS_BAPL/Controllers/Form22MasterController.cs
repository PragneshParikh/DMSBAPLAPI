using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.Form22Services;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Form22MasterController : ControllerBase
    {
        private readonly IForm22Service _form22Service;

        string message = string.Empty;
        public Form22MasterController(IForm22Service form22Service)
        {
            _form22Service = form22Service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertForm22Master([FromBody] Form22MasterViewModel form22MasterViewModel)
        {
            try
            {
                if (form22MasterViewModel == null)
                {
                    return BadRequest("Form22Master data is required.");
                }
                var result = await _form22Service.InsertForm22MasterAsync(form22MasterViewModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                message = $"An error occurred while processing the request: {ex.Message}";
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        [HttpGet("GetOemModelList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOemModelList()
        {
            try
            {
                var result = await _form22Service.GetOemModelListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                message = $"An error occurred while processing the request: {ex.Message}";
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetForm22Masters(string? search)
        {
            try
            {
                var result = await _form22Service.GetForm22MastersAsync(search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                message = $"An error occurred while processing the request: {ex.Message}";
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetForm22MasterById(int id)
        {
            try
            {
                var result = await _form22Service.GetForm22MasterByIdAsync(id);
                if (result == null)
                {
                    return NotFound($"Form22Master with ID {id} not found.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                message = $"An error occurred while processing the request: {ex.Message}";
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateForm22Master(int id, [FromBody] Form22MasterViewModel form22MasterViewModel)
        {
            try
            {
                var existingForm22Master = await _form22Service.UpdateForm22MasterAsync(id, form22MasterViewModel);
                if (existingForm22Master == null)
                {
                    return NotFound($"Form22Master with ID {id} not found.");
                }
                return Ok(new
                {
                    Message = StringConstants.AggregateTaxCodeUpdated,
                    Data = existingForm22Master
                });
            }
            catch (Exception ex)
            {
                message = $"An error occurred while processing the request: {ex.Message}";
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        [HttpGet("download")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download()
        {
            try
            {
                var file = await _form22Service.DownloadForm22MasterExcel();
                if (file == null)
                {
                    return NotFound("The requested file could not be found.");
                }
            }
            catch (Exception ex)
            {
                message = $"An error occurred while processing the request: {ex.Message}";
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}
