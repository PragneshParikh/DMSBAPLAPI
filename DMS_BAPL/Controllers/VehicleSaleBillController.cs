using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
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
        private readonly IPartInventoryService _partInventoryService;
        public VehicleSaleBillController(IVehicleSaleBillService vehicleSaleBill, IPartInventoryService partInventoryService)
        {
            _vehicleSaleBillService = vehicleSaleBill;
            _partInventoryService = partInventoryService;
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
        public async Task<IActionResult> GetAll(string? dealerCode, string? search, DateTime? fromDate, DateTime? toDate, string? erpStatus)
        {
            try
            {
                return Ok(await _vehicleSaleBillService.GetAllAsync(dealerCode, search, fromDate, toDate, erpStatus));
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

                return Ok(new
                {
                    success = true,
                    message = "Deleted Successfully"
                });
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
        public async Task<IActionResult> ConvertPO(string dealerCode,int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(StringConstants.PORequired);

                var result = await _vehicleSaleBillService.GetExportDetails(dealerCode,id);

                if (result == null)
                    return NotFound(StringConstants.PONotFound);

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


        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("ChassisListPDIOK")]
        public async Task<IActionResult> GetAllChassisListWithPDIOK(string dealerCode, int ledgerId)
        {
            try
            {
                return Ok(await _vehicleSaleBillService.GetPdiVehiclesAsync(dealerCode, ledgerId));
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
        [HttpGet("PreCheckDelete")]
        public async Task<IActionResult> GetAllChassisListWithPDIOK(int saleBillId)
        {
            try
            {
                var result = await _vehicleSaleBillService.GetVehicleDeletionPreRequisiteCheck(saleBillId);
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

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("ChassisList")]
        public async Task<IActionResult> GetAllChassisList(string? dealerCode, int ledgerId,string locCode)
        {
            try
            {
                return Ok(await _vehicleSaleBillService.GetAllChassissListWithPDISatatus(dealerCode, ledgerId,locCode));
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


        [HttpPut("ConfirmInvoice")]
        public async Task<int> ConfirmInvoiceAndReserveChassis(string saleBillNo)
        {
            try
            {
                return await _vehicleSaleBillService.ConfirmInvoiceAndReserveChassis(saleBillNo);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPut("updateRegistration")]
        public async Task<VehicleSaleBillHeader> UpdateRegistrationAndReserveChassis(string? saleBillNo, List<UpdateSaleDetailsVM> updateSaleDetails)
        {
            try
            {
                return await _vehicleSaleBillService.UpdateRegistrationAndReserveChassis(saleBillNo, updateSaleDetails);
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
        public async Task<IActionResult> Download(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {

                var file = await _vehicleSaleBillService.DownloadSaleBillExcel(dateFrom, dateTo);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "SaleBillList.xlsx"
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

        [HttpGet("PolicyNos/{chassisNo}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPolicyNos(string chassisNo)
        {
            try
            {
                var policyNos = await _vehicleSaleBillService.GetPolicyNo(chassisNo);
                return Ok(policyNos);
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

        [HttpGet("Download/{id}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadSaleBill(int id)
        {
            try
            {
                var pdfBytes = await _vehicleSaleBillService.DownloadSaleBillPdf(id);

                return File(
                    pdfBytes,
                    "application/pdf",
                    $"SaleBill_{id}.pdf"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("DownloadMultiple")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadMultiple([FromBody] List<int> ids)
        {
            try
            {
                var zipBytes =
                    await _vehicleSaleBillService.DownloadMultipleSaleBills(ids);

                return File(
                    zipBytes,
                    "application/zip",
                    "SaleBills.zip"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("DownloadMultipleForm22")]
        public async Task<IActionResult> DownloadMultipleForm22([FromBody] List<int> ids)
        {
            try
            {
                var zipBytes = await _vehicleSaleBillService.DownloadMultipleForm22(ids);
                return File(zipBytes, "application/zip", "Form22_Bills.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("DownloadMultipleInvoices")]
        public async Task<IActionResult> DownloadMultipleInvoices([FromBody] List<int> ids)
        {
            try
            {
                var zipBytes = await _vehicleSaleBillService.DownloadMultipleInvoices(ids);
                return File(zipBytes, "application/zip", "ExShowroom_Invoices.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("DownloadMultipleCombined")]
        public async Task<IActionResult> DownloadMultipleCombined([FromBody] CombinedDownloadRequest request)
        {
            try
            {
                var zipBytes = await _vehicleSaleBillService.DownloadMultipleCombined(
                    request.Form22Ids,
                    request.InvoiceIds
                );
                return File(zipBytes, "application/zip", "SaleBills.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}