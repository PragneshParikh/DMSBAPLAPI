using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LeadMasterRepo;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Data.Services.LeadMasterService;
using DMS_BAPL_Data.ViewModels;
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
        public async Task<IActionResult> InsertLMSLead([FromBody] List<LeadViewModel> leadViewModels )
        {
            foreach (var lmsLeadData in leadViewModels)
            {
                await _leadMasterService.InsertLmsleadMasterAsync(lmsLeadData);
            }

            return Ok("LMS Customer  details inserted Successfully !");
        }

        // GET api/itemMaster

        [HttpGet]
        public async Task<IActionResult> GetAllLMSLead()
        {
            var items = await _leadMasterService.GetAlllmsleadMasters();
            return Ok(items);



        }
    }
}
