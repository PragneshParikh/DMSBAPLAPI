using DMS_BAPL_Data.Services.Form22Services;
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

        public Form22MasterController(IForm22Service form22Service)
        {
            _form22Service = form22Service;
        }

        [HttpPost]
        public async Task<IActionResult> InsertForm22Master([FromBody] Form22MasterViewModel form22MasterViewModel)
        {
            var result = await _form22Service.InsertForm22MasterAsync(form22MasterViewModel);
            return Ok("FORM22Master Details added." + result);
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
        public async Task<IActionResult> UpdateForm22Master(int id, [FromBody] Form22MasterViewModel form22MasterViewModel)
        {
            var existingForm22Master = await _form22Service.GetForm22MasterByIdAsync(id);
            if (existingForm22Master == null)
            {
                return NotFound($"Form22Master with ID {id} not found.");
            }

            // Update the existing Form22Master with new values
            existingForm22Master.SoundLevelHorn = form22MasterViewModel.SoundLevelHorn;
            existingForm22Master.PassbyNoiseLevel = form22MasterViewModel.PassbyNoiseLevel;
            existingForm22Master.ApprovalCertificateNo = form22MasterViewModel.ApprovalCertificateNo;
            existingForm22Master.CreatedBy = form22MasterViewModel.CreatedBy;
            existingForm22Master.CreatedDate = DateTime.UtcNow;

            var updatedForm22Master = await _form22Service.UpdateForm22MasterAsync(existingForm22Master);
            return Ok("FORM22 Master details updated." + updatedForm22Master);


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
