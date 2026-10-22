using DMS_BAPL_Data.Repositories.WarrantyPackingRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyDispatchController : ControllerBase
    {
        private readonly IWarrantyPackingRepo _warrantyPackingRepo;

        public WarrantyDispatchController(IWarrantyPackingRepo warrantyPackingRepo)
        {
            _warrantyPackingRepo = warrantyPackingRepo;
        }

        [HttpPost("SearchWarrantyPackingSlips")]
        public async Task<IActionResult> SearchWarrantyPackingSlips([FromBody] WarrantyPackingSlipSearchViewModel filter)
        {
            try
            {
                var result = await _warrantyPackingRepo.SearchWarrantyPackingSlips(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("SaveWarrantyDispatch")]
        public async Task<IActionResult> SaveWarrantyDispatch([FromBody] WarrantyDispatchViewModel model)
        {
            try
            {
                string userId = User.Identity?.Name ?? "Admin";
                var id = await _warrantyPackingRepo.SaveWarrantyDispatch(model, userId);
                return Ok(new { success = true, id = id, message = "Warranty Dispatch saved successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("SearchWarrantyDispatch")]
        public async Task<IActionResult> SearchWarrantyDispatch([FromBody] WarrantyDispatchSearchViewModel filter)
        {
            try
            {
                var result = await _warrantyPackingRepo.SearchWarrantyDispatch(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }  
    
}
