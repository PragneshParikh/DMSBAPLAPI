using DMS_BAPL_Data.Services.HSNCodeMaterService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HSNCodeMasterController : ControllerBase
    {
        private readonly IHSNCodeMaterService _hSNCodeMaterService;
        public HSNCodeMasterController(IHSNCodeMaterService hSNCodeMaterService)
        {
            _hSNCodeMaterService = hSNCodeMaterService;
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetAll(string? search)
        {
            try
            {
                return Ok(await _hSNCodeMaterService.GetAllHSNCodeListAsync(search));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("update/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {

                var data = await _hSNCodeMaterService.GetByIdAsync(id);

                if (data == null)
                    return NotFound();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(HSNCodeMasterViewModel model)
        {
            try
            {
                var result = await _hSNCodeMaterService.AddAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, HSNCodeMasterViewModel model)
        {
            try
            {
                var result = await _hSNCodeMaterService.UpdateAsync(id, model);
                return Ok(new
                {
                    message = StringConstants.HSNCodeUpdatedSuccessfully,
                    data = result
                });
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {
            try
            {
            var file = await _hSNCodeMaterService.downloadHSNCodeExcel();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "HSNCodeMasterList.xlsx"
            );
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


    }
}
