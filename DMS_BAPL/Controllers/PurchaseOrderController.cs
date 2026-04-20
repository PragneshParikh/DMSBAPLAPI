using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.PurchaseOrder;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
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
                var result = await _purchaseOrderService.GetPOByNumberAsync(code);
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

        /// <summary>
        /// Retrieves the list of all purchase orders.
        /// </summary>
        /// <returns>List of purchase orders</returns>
        [HttpGet("Polist")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetPOList()
        {
            try
            {
                var result = await _purchaseOrderService.GetPOListAsync();
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
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
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
        public async Task<IActionResult> ConvertPO([FromBody] string poNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(poNumber))
                    return BadRequest(StringConstants.PORequired);

                var result = await _purchaseOrderService.ConvertPOToERPJsonAsync(poNumber);

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
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(StringConstants.UserUnauthorized);

                var result = await _purchaseOrderService.UpdatePOAsync(model, userId);

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
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("subsidy")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubsidy()
        {
            try
            {
                var result = await _purchaseOrderService.GetSubsidyValueAsync();
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

        #region Parts PO Endpoints

        [HttpGet("parts/Polist")]
        [ProducesResponseType(typeof(IEnumerable<PartsPurchaseOrderResponseViewModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPartsPOList()
        {
            try
            {
                var result = await _purchaseOrderService.GetPartsPOListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("parts/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePartsPurchaseOrder([FromBody] PartsPurchaseOrderViewModel model)
        {
            if (model == null) return BadRequest(StringConstants.BadRequest);
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId)) return Unauthorized(StringConstants.UserUnauthorized);

                var result = await _purchaseOrderService.CreatePartsPOAsync(model, userId);
                if (result)
                {
                    return Ok(new { success = true, message = StringConstants.POCreated, poNumber = model.PONumber });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = StringConstants.POCreatedPOCreationailed });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
