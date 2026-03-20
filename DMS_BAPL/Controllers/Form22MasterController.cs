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
        public async Task<IActionResult> InsertForm22Master([FromBody] Form22MasterViewModel form22MasterViewModel)
        {
            var result = await _form22Service.InsertForm22MasterAsync(form22MasterViewModel);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetForm22Masters(string? search)
        {
            var result = await _form22Service.GetForm22MastersAsync(search);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetForm22MasterById(int id)
        {
            var result = await _form22Service.GetForm22MasterByIdAsync(id);
            if (result == null)
            {
                return NotFound($"Form22Master with ID {id} not found.");
            }
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateForm22Master(int id, [FromBody] Form22Master form22MasterViewModel)
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
        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {
            var file = await _form22Service.DownloadForm22MasterExcel();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Form22MasterList.xlsx"
            );
        }


    }
}
