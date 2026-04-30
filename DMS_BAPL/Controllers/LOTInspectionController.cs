using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Services.LOTInspectionService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LOTInspectionController : ControllerBase
    {
        private readonly ILotInspectionService _invoiceService;
        private readonly ILotInspectionDetailsService _lotInspectionDetailsService;
        private readonly ILogger<LOTInspectionController> _logger;
        public LOTInspectionController(ILotInspectionDetailsService lotInspectionDetailsService, ILotInspectionService invoiceService, ILogger<LOTInspectionController> logger)
        {
            _invoiceService = invoiceService;
            _lotInspectionDetailsService = lotInspectionDetailsService;
            _logger = logger;
        }

        //Summary: Insert invoice details in LOT inspection header table based on invoice no
        [HttpPost]
        [Route("AcceptInvoices")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptInvoices([FromBody] string invoiceNo)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found");

                var result = await _invoiceService.InsertLotInspectionHeaderAsync(invoiceNo, userId);

                if (result > 0)
                {
                    await _lotInspectionDetailsService.InsertLotDetailsByInvoiceNo(invoiceNo, result, userId);
                }

                return Ok(new
                {
                    Message = "Invoices inserted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting invoices header details in DB for invoice number: {InvoiceNo}", invoiceNo);
                throw; // Rethrow the exception to be handled by global exception handler
            }
        }

        //Summary: Update invoice details in LOT inspection header table based on invoice no
        [HttpPut]
        [Route("UpdateLotInspectedDetails")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLotInspectedDetails(LotInspectionViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                string dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found");

                var result = await _invoiceService.UpdateLotInspectionAsync(model, userId, dealerCode);

                return Ok(new
                {
                    Message = "Updated successfully",
                    Success = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Updating invoices header and other details in DB : {model}", model);
                throw; // Rethrow the exception to be handled by global exception handler
            }
        }
        //Summary: Get list of all accepted invoice
        [HttpGet]
        [Route("GetAllAcceptedInvoiceList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAcceptedInvoiceList(string? search)
        {
            try
            {
                var result = await _invoiceService.GetAllLotInspectionHeaderDetailsAsync(search);
                return Ok(new
                {
                    Message = "All accepted invoiceHeader list retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all accepted invoice list");
                throw; // Rethrow the exception to be handled by global exception handler
            }
        }
    }
}
