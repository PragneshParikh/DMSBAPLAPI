using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Data.Services.PurchaseOrder;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using MailKit.Net.Imap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPrefixService _prefixService;
        private readonly ILogger<PurchaseOrderController> _logger;
        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService, IPrefixService prefixService, ILogger<PurchaseOrderController> logger)
        {
            _purchaseOrderService = purchaseOrderService;
            _prefixService = prefixService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a purchase order by its unique code/number.
        /// </summary>
        /// <param name="code">Purchase order code</param>
        /// <returns>Purchase order details if found</returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetPurchaseOrderByCode(string code)
        {
            try
            {
                string dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

                if (string.IsNullOrEmpty(dealerCode))
                    return Unauthorized("User not authorized");

                var result = await _purchaseOrderService.GetPOByNumberAsync(code);

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of all purchase orders.
        /// </summary>
        /// <returns>List of purchase orders</returns>
        [HttpGet("Polist")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetPOList(string? dealerCode, string orderType, int pageIndex, int pageSize, [FromQuery] PurchaseOrderSearchViewModel purchaseOrderSearchViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _purchaseOrderService.GetPOListAsync(dealerCode, orderType, pageIndex, pageSize, purchaseOrderSearchViewModel);

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a new purchase order.
        /// </summary>
        /// <param name="model">Purchase order data</param>
        /// <returns>Success response with PO number if created</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderViewModel model)
        {
            if (model == null)
                return BadRequest(StringConstants.BadRequest);

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(StringConstants.UserUnauthorized);

                var result = await _purchaseOrderService.CreatePOAsync(model, userId);

                if (result)
                {
                    await _prefixService.UpdateNextNumberByDealerByModule(model.CustomerCode, "purchase_order");
                    return Ok(new
                    {
                        success = true,
                        message = StringConstants.POCreated,
                        poNumber = model.PONumber
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = StringConstants.POCreatedPOCreationailed
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Convert Purchase Order to ERP JSON format
        /// </summary>
        /// <param name="poNumber">PO Number</param>
        /// <returns>ERP JSON</returns>
        [HttpPost("SendToERP")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConvertPO([FromBody] object erpObject)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                //if (string.IsNullOrEmpty(erpObject))
                //    return BadRequest(StringConstants.PORequired);

                var result = await _purchaseOrderService.ConvertPOToERPJsonAsync(erpObject);

                if (result == null)
                    return NotFound(StringConstants.PONotFound);

                return Ok(result); // returns JSON
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates an existing purchase order.
        /// </summary>
        /// <param name="model">Purchase order data</param>
        /// <returns>Success response</returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePurchaseOrder([FromBody] PurchaseOrderViewModel model)
        {
            if (model == null)
                return BadRequest(StringConstants.BadRequest);

            try
            {
                string dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

                if (string.IsNullOrEmpty(dealerCode))
                    return Unauthorized(StringConstants.UserUnauthorized);

                var result = await _purchaseOrderService.UpdatePOAsync(model, dealerCode);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = StringConstants.POUpdated,
                        poNumber = model.PONumber
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = StringConstants.POUpdateFailed
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut("UpdatePOStatus")]
        [ProducesResponseType(typeof(Boolean), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> UpdatePOStatus([FromBody] UpdatePOStatusViewModel updatePOStatusViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var poStatus = await _purchaseOrderService.UpdatePOStatusAsync(updatePOStatusViewModel);

                return Ok(poStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PO status for PO Number: {PONumber}", updatePOStatusViewModel.PONumber);
                throw;
            }
        }

        /// <summary>
        /// Deletes all items and taxes for a purchase order (preserving the header).
        /// </summary>
        /// <param name="poNumber">PO Number</param>
        /// <returns>Success response</returns>
        [HttpDelete("items/{poNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePurchaseOrderItems(string poNumber)
        {
            if (string.IsNullOrEmpty(poNumber))
                return BadRequest(StringConstants.PORequired);

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _purchaseOrderService.DeletePOItemsAsync(poNumber);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = StringConstants.POItemsDeleted
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = StringConstants.PODeleteFailed
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// Get Subsidy Value
        /// </summary>
        /// <returns></returns>
        [HttpGet("subsidy")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubsidy()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _purchaseOrderService.GetSubsidyValueAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region Parts PO Endpoints
        /// <summary>
        /// Get Parts PO List Async
        /// </summary>
        /// <returns></returns>
        //[HttpGet("parts/Polist")]
        //[ProducesResponseType(typeof(IEnumerable<PartsPurchaseOrderResponseViewModel>), StatusCodes.Status200OK)]
        //public async Task<ActionResult> GetPartsPOList()
        //{
        //    try
        //    {
        //        var result = await _purchaseOrderService.GetPartsPOListAsync();
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        //    }
        //}
        /// <summary>
        /// Create Parts Purchase Order
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        //[HttpPost("parts/create")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<IActionResult> CreatePartsPurchaseOrder([FromBody] PurchaseOrderViewModel model)
        //{
        //    if (model == null) return BadRequest(StringConstants.BadRequest);
        //    try
        //    {
        //        string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

        //        if (string.IsNullOrEmpty(userId))
        //            return Unauthorized(StringConstants.UserUnauthorized);

        //        var result = await _purchaseOrderService.CreatePartsPOAsync(model);
        //        if (result)
        //        {
        //            return Ok(new { success = true, message = StringConstants.POCreated, poNumber = model.PONumber });
        //        }
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = StringConstants.POCreatedPOCreationailed });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        //    }
        //}

        #endregion
        /// <summary>
        /// Download Vehicle Purchase Order Excel
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("DownloadPurchaseOrderExcel")]
        public async Task<IActionResult> DownloadPurchaseOrderExcel([FromQuery] PurchaseOrderSearchViewModel filter)
        {
            try
            {
                var fileBytes = await _purchaseOrderService.DownloadPurchaseOrderExcel(filter);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "VehiclePurchaseOrders.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetItemDetailsByItemCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderDetailsByItemCode([FromQuery] string itemCode, string dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User not authorized");

                var result = await _purchaseOrderService.GetOrderDetailsByItemCode(itemCode, dealerCode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
