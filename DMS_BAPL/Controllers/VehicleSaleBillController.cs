using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.VehicleSaleBillService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleSaleBillController : ControllerBase
    {
        private readonly IVehicleSaleBillService _vehicleSaleBillService;
        public VehicleSaleBillController(IVehicleSaleBillService vehicleSaleBill)
        {
           _vehicleSaleBillService = vehicleSaleBill; 
        }

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(VehicleSaleBillEditCreateViewModel model)
        {
            try
            {
                var id = await _vehicleSaleBillService.CreateAsync(model);
                return Ok(id);
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _vehicleSaleBillService.GetByIdAsync(id);
                if (result == null) return NotFound();
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? search,DateTime? fromDate, DateTime? toDate,string? erpStatus)
        {
            try
            {
                return Ok(await _vehicleSaleBillService.GetAllAsync(search,fromDate,toDate,erpStatus));
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VehicleSaleBillEditCreateViewModel model)
        {
            try
            {
             await _vehicleSaleBillService.UpdateAsync(id, model);
            return Ok();
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

        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
            await _vehicleSaleBillService.DeleteAsync(id);
            return Ok("Deleted Successfully");
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

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("getNextSaleBillNo")]
        public async Task<IActionResult> GetNextReceiptNo()
        {
            try
            {
                var receiptNo = await _vehicleSaleBillService.GenerateNextVehicleSaleNo();
                return Ok(receiptNo);

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

        [HttpPost("SendToERP")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConvertPO([FromBody] int saleBillId)
        {
            try
            {
                if (saleBillId <= 0)
                    return BadRequest(StringConstants.PORequired);

                var result = await _vehicleSaleBillService.GetExportData(saleBillId);

                if (result == null)
                    return NotFound(StringConstants.PONotFound);

                return Ok(result); // returns JSON
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

        [HttpGet("GetChasisPricing")]
        public async Task<IActionResult> GetChasisPricing(string dealerCode, int ledgerId)
        {
            var request = new VehicleSaleChasisRequest
            {
                DealerCode = dealerCode,
                LedgerId = ledgerId
            };

            var result = await _vehicleSaleBillService.GetChasisList(request);
            return Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("ChassisListPDIOK")]
        public async Task<IActionResult> GetAllChassisListWithPDIOK(string dealerCode)
        {
            try
            {
                return Ok(await _vehicleSaleBillService.GetPdiVehiclesAsync(dealerCode));
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
