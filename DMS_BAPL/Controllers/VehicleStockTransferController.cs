using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.VehicleStockTransferService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleStockTransferController : ControllerBase
    {
        private readonly IVehicleStockTransferService _vehicleStockTransferService;
        public VehicleStockTransferController(IVehicleStockTransferService vehicleStockTransferService)
        {
            _vehicleStockTransferService = vehicleStockTransferService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(VehicleStockTransferCreateEditViewModel model)
        {
            try
            {
                var result = await _vehicleStockTransferService.CreateAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet("list")]

        public async Task<IActionResult> GetVehicleStockTransferList([FromQuery]VehicleStockTransferFilterViewModel filter)
        {
            try
            {
                var result = await _vehicleStockTransferService.GetVehicleStockTransfer(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransferById(int id)
        {
            try
            {
                var result = await _vehicleStockTransferService.GetVehicleTransferById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Downloads dealer list as an Excel file.
        /// </summary>
        /// <returns>Excel file containing dealer data</returns>
        [HttpGet("download")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download(DateTime? dateFrom = null, DateTime? dateTo = null, string? issuingLocation = null, string? receivingLocation = null, string? search = null)
        {
            try
            {

                var file = await _vehicleStockTransferService.DownloadTransferExcel(dateFrom, dateTo,issuingLocation,receivingLocation,search);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "TransferStock.xlsx"
                );
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
