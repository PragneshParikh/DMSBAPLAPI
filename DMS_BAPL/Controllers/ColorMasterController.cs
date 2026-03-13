using DMS_BAPL_Data;
using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.ViewModels;
using DMS_BAPL_Utils.Helpers;
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
        /// Retrieves the full list of colors.
        /// </summary>
        /// <remarks>
        /// This endpoint returns all colors available in the system for the authenticated user.
        /// If no colors are found, it returns a 204 No Content response.
        /// </remarks>
        /// <returns>
        /// Returns an <see cref="IEnumerable{ColorMaster}"/> containing the list of colors.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ColorMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ColorMaster>>> GetColors()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var colors = await _colorMasterService.GetColors();

                return Ok(colors);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        /// <summary>
        /// Retrieves a paginated list of colors along with the total count.
        /// </summary>
        /// <param name="searchTerm">Optional search string to filter colors by name.</param>
        /// <param name="pageIndex">Page number (starting from 1).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>A <see cref="PagedResponse{ColorMaster}"/> containing the list of colors and total count.</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponse<ColorMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetColorByPaged([FromQuery] string? searchTerm, [FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var colors = await _colorMasterService.getColorsByPaged(searchTerm, pageIndex, pageSize);

                return Ok(colors);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add new color into the table
        /// This prevent from adding duplicate color and the field name is "colorname"
        /// </summary>
        /// <param name="colorMasterViewModel"></param>
        /// <returns></returns>
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
