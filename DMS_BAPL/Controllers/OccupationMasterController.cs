using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.OccupationMasterService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OccupationMasterController : ControllerBase
    {
        private readonly IOccupationMasterService _service;
        public OccupationMasterController(IOccupationMasterService service)
        {
            _service = service;
        }

        [HttpPost("AddOccupationMaster")]
        public async Task<IActionResult> AddOccupationMaster(OccupationViewModel occupationViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                var result = await _service.AddOccupationMasterAsync(occupationViewModel, userId);

                return Ok(new
                {
                    Success = true,
                    Message = "Occupation added successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetOccupationMasters")]
        public async Task<IActionResult> GetOccupationMasters()
        {
            try
            {
                var result = await _service.GetOccupationMastersAsync();

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("UpdateOccupationMaster/{id}")]
        public async Task<IActionResult> UpdateOccupationMaster(int id, OccupationViewModel occupationViewModel)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _service.UpdateOccupationMasterAsync(
                        id,
                        occupationViewModel,
                        userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Occupation not found."
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Occupation updated successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetPaginatedOccupationMasters")]
        public async Task<IActionResult> GetPaginatedOccupationMasters(string? occupationName, int? page, int? pageSize)
        {
            try
            {
                var result = await _service.GetPaginatedOccupationMastersAsync(occupationName, page, pageSize);

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetActiveOccupation")]
        public async Task<IActionResult> GetActiveOccupations()
        {
            try
            {
                var result = await _service.GetOccupationMastersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }

}