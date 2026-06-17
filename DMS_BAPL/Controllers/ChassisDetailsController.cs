using DMS_BAPL_Data.Services.ChassisDetailsService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/chassis-details")]
    [ApiController]
    public class ChassisDetailsController : ControllerBase
    {
        private readonly IChassisDetailService _chassisDetailService;
        public ChassisDetailsController(IChassisDetailService chassisDetailService)
        {
            _chassisDetailService = chassisDetailService;
        }

        [HttpGet("chassisList")]
        public async Task<IActionResult> GetChassisDetailBylLocationCode(string locationCode)
        {
            try
            {
                var result = await _chassisDetailService.GetChassisList(locationCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
