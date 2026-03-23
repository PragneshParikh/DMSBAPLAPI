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
        [HttpGet]
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
    }
}
