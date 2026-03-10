using DMS_BAPL_Data;
using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/color")]
    [ApiController]
    public class ColorMasterController : ControllerBase
    {
        private readonly IColorMasterService _colorMasterService;

        public ColorMasterController(IColorMasterService colorMasterService)
        {
            _colorMasterService = colorMasterService;
        }

        /// <summary>
        /// Get the list of colors
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetColors()
        {
            List<ColorMaster>? colors = null;
            try
            {
                colors = await _colorMasterService.GetColors();

                if (colors is null)
                    return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok(colors);
        }

        [HttpPost]
        public async Task<ActionResult> InsertColor(ColorMasterViewModel colorMasterViewModel)
        {
            try
            {
                if (colorMasterViewModel == null)
                {
                    return BadRequest("Color data is required.");
                }

                var color = await _colorMasterService.CreateColor(colorMasterViewModel);

                if (color == null)
                {
                    // Duplicate case
                    var response = new ApiResponse
                    {
                        Valid = false,
                        Description = "Color already exists.",
                        Value = new List<ResponseValue>
                        {
                            new ResponseValue
                            {
                                Msg = "Color already exists.",
                                StatusCode = "409",
                                ResponseStatus = "false"
                            }
                        }
                    };
                    return Conflict(response);
                }

                // Success case - include inserted ID
                var successResponse = new ApiResponse
                {
                    Valid = true,
                    Description = "Color Saved Successfully.",
                    Value = new List<ResponseValue>
                    {
                        new ResponseValue
                        {
                            Msg = $"Color Saved Successfully. ID: {color.rrgcoloridno}",
                            StatusCode = "200",
                            ResponseStatus = "true"
                        }
                    }
                };

                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
