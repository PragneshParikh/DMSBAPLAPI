using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.APITracking;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/api-tracking")]
    [ApiController]
    public class APITrackingController : ControllerBase
    {
        private readonly IAPITrackingRepo _apiTrackingRepo;

        public APITrackingController(IAPITrackingRepo apiTrackingRepo)
        {
            _apiTrackingRepo = apiTrackingRepo;
        }

        [HttpGet]
        public async Task<ActionResult> GetAPITracking()
        {
            List<Apitracking>? apiTracking = null;
            try
            {
                apiTracking = await _apiTrackingRepo.GetAPITracking();

                // Implement logic to retrieve API tracking data
                return Ok(apiTracking);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("FilterData")]
        public async Task<ActionResult> GetFilterData([FromQuery] DateTime fromDate, [FromQuery] DateTime ToDate, [FromQuery] string endPoint = "", [FromQuery] string status = "")
        {
            List<Apitracking>? apitrackings = null;
            try
            {
                apitrackings = await _apiTrackingRepo.GetFilterRecords(fromDate, ToDate, endPoint, status);
                return Ok(apitrackings);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
