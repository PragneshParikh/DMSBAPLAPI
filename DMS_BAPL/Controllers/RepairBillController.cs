using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.RepairBillRepo;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DMS_BAPL_Utils.ViewModels.RepairBillViewModel;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairBillController : ControllerBase
    {
        private readonly IRepairBillRepo _repairBillRepo;
        private readonly ILogger<RepairBillController> _logger;
        private readonly IPrefixService _prefixService;

        public RepairBillController(IRepairBillRepo repairBillRepo, ILogger<RepairBillController> logger, IPrefixService prefixService)
        {
            _repairBillRepo = repairBillRepo;
            _logger = logger;
            _prefixService = prefixService;
        }

        [HttpPost("GetAllRepairBillList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRepairBillList([FromBody] RepairBillSearchVM? search)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                search ??= new RepairBillSearchVM();
                var result = await _repairBillRepo.GetAllRepairBillList(search);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchJobCard");
                return StatusCode(500, "An error occurred while searching job cards.");
            }

        }

        [HttpPost("InsertRepairBill")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertRepairBill(RepairBillViewModel.RepairBillInsertVM model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");



                var result = await _repairBillRepo.InsertRepairBill(model, userId);
                if(result > 0)
                {
                    await _prefixService.UpdateNextNumberByDealerByModule(model.RepairBillheader.DealerCode, "Repair_bill");
                    return Ok(new { Success = true, RepairBillId = result });
                }
                else
                {
                    _logger.LogError("Failed to insert repair bill.");
                    return StatusCode(500, new { Success = false, Message = "An error occurred while inserting the repair bill."});
                   
                }

            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while inserting the repair bill.");
                return StatusCode(500, new { Success = false, Message = "An error occurred while inserting the repair bill.", Details = ex.Message });
            }
        }

        [HttpGet("GetRepairBillById/{id}")]
        public async Task<IActionResult> GetRepairBillById(int id)
        {
            var result = await _repairBillRepo.GetRepairBillById(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }




    }
}
