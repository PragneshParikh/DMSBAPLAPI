using DMS_BAPL_Data.Repositories.ServiceHeadRepo;
using DMS_BAPL_Data.Repositories.VehicleOpeningStockRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleOpeningStockController : ControllerBase
    {
        private readonly IVehicleOpeningStock _vehicleOpeningStock;
        private readonly ILogger<VehicleOpeningStockController> _logger;

        public VehicleOpeningStockController(IVehicleOpeningStock vehicleOpeningStock, ILogger<VehicleOpeningStockController> logger)
        {
            _vehicleOpeningStock = vehicleOpeningStock;
            _logger = logger;
        }


        [HttpGet("GetVehicleSaleDetailsByModel/{modelName}/{dealerCode}")]
        [ProducesResponseType(typeof(List<VehicleOpeningDetailsVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VehicleOpeningStockDetails(string? modelName,string? dealerCode)
        {
            try
            {
                var result = await _vehicleOpeningStock.GetVehicleSaleDetailsByModelAsync(modelName,dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVehicleSaleDetailsByModel");
                return StatusCode(500, "An error occurred while retrieving VehicleSaleDetailsByModel.");
            }
                  
        }
    }
}
