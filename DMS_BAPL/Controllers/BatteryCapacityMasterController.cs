using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.BatteryCapacityMasterService;
using DMS_BAPL_Utils;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatteryCapacityMasterController : ControllerBase
    {
        private readonly IBatteryCapacityMasterService _batteryCapacityMasterService;

        public BatteryCapacityMasterController(IBatteryCapacityMasterService batteryCapacityMasterService)
        {
            _batteryCapacityMasterService = batteryCapacityMasterService;

        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBatterCapacityMaster([FromBody] BatteryCapacityMasterViewModel batteryCapacityMasterViewModel)
        {
            var result = await _batteryCapacityMasterService.AddBatteryCapacityMasterAsync(batteryCapacityMasterViewModel);

            return Ok(new
            {
                message = StringConstants.BatteryCapacityMasterCreated,
                data = result
            });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllBatteryCapacityMasterAsync()
        {
            var batteryCapacityMasters = await _batteryCapacityMasterService.GetBatteryCapacityMastersAsync();

            return Ok(new
            {
                message = StringConstants.DealerFetched,
                data = batteryCapacityMasters ?? new List<BatteryCapacityMaster>()
            });
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBatteryCapacityMasterAsync(int id, [FromBody] BatteryCapacityMasterViewModel batteryCapacityMasterViewModel)
        {
            var result = await _batteryCapacityMasterService.UpdateBatteryCapacityMasterAsync(id, batteryCapacityMasterViewModel);

            if (result == null)
            {
                return NotFound(StringConstants.BatteryCapacityMasterUpdateFailed);
            }

            return Ok(new
            {
                message = StringConstants.BatteryCapacityMasterUpdated,
                data = result
            });
        }


        [HttpGet("listPaginated")]
        public async Task<IActionResult> GetPaginatedResultAsync(
      string? batteryCapacity,
      int? page = null,
      int? pageSize = null)
        {
            var result = await _batteryCapacityMasterService
                .GetPaginatedBatteryCapacityMastersAsync(batteryCapacity, page, pageSize);

            if (result == null || result.Data == null || !result.Data.Any())
            {
                return Ok(new
                {
                    message = "No records found",
                    totalRecords = 0,
                    data = new List<BatteryCapacityMaster>()
                });
            }

            return Ok(new
            {
                message = "Battery Capacity fetched successfully",
                totalRecords = result.TotalRecords,
                data = result.Data
            });
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {
            var file = await _batteryCapacityMasterService.DownloadDealerExcel();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Battery Capacity Master List.xlsx"
            );
        }
    }
}