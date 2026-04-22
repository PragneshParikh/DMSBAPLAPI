using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LeadMasterRepo;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Data.Services.LeadMasterService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LMSLeadMasterController : ControllerBase
    {

        private readonly ILeadMasterService _leadMasterService;

        public LMSLeadMasterController(ILeadMasterService leadMasterService)
        {
            _leadMasterService = leadMasterService;
        }

        // POST api/itemMaster
        [HttpPost]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertLMSLead([FromBody] LeadViewModel leadViewModel)
        {
            if (leadViewModel == null)
            {
                return BadRequest("Lead data is required.");
            }

            try
            {
                // Better: send whole list to service (bulk insert)
                await _leadMasterService.InsertLmsleadMasterAsync(leadViewModel);

                return Ok(new { message = "LMS Customer details inserted successfully!" });
            }
            catch (Exception ex)
            {
                // Log error here (important)
                return StatusCode(500, "Something went wrong while inserting data.");
            }
        }

        // GET api/itemMaster

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllLMSLead()
        {
            try
            {
                var items = await _leadMasterService.GetAlllmsleadMasters();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("lmsLeadbyMob")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLMSLeadByMoborBookingId(string? mobileNo,int? bookingId)
        {
            try
            {
                var item = await _leadMasterService.GetLMSLeadMasterByMobileNo(mobileNo, bookingId);
                return Ok(new
                {
                    lead = item.lead,
                    ledgerId = item.ledgerId,
                    isNew = item.isNew
                });
            }
            catch (Exception ex){
                return BadRequest(ex.Message);
            }
        }
    }
}
