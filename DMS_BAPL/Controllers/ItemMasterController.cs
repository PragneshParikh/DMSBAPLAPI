using DMS_BAPL_Data;
using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ItemMasterController : ControllerBase
    {
        private readonly IitemMasterService _itemMasterService;

        public ItemMasterController(IitemMasterService itemservice)
        {
            _itemMasterService = itemservice;
        }

        // POST api/itemMaster
        [HttpPost]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertItem([FromBody] insertItemMasterViewModel items)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found");

                var result = await _itemMasterService.InsertItemAsync(items, userId);
                return Ok(new
                {

                    Message = "Item Master details inserted successfully",
                    Data = result
                });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // GET api/itemMaster
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllItems([FromQuery] int? grpidno, string? search)
        {
            try
            {
                var items = await _itemMasterService.GetAllItemMastersAsync(grpidno, search);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // PUT api/itemMaster/{id}
        //[HttpPut("{id}")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemMaster item)
        {
            try
            {
                item.Id = id;
                await _itemMasterService.UpdateItemAsync(item);
                return Ok("Item updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("download")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download()
        {
            try
            {
                var file = await _itemMasterService.DownloadItemMasterExcel();

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ItemMasterList.xlsx"
                    );
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("GetPurchaseDetailsByModelNo/{modelNo}")]
        [ProducesResponseType(typeof(ItemMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPurchaseDetailsByModelNo(string modelNo)
        {
            try
            {
                var data = await _itemMasterService.GetPurchaseDetailsByModelNo(modelNo);

                if (data == null)
                    return NotFound("Model not found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Get Purchase Details With HsnTax ByModelNo
        /// </summary>
        /// <param name="modelNo"></param>
        /// <returns></returns>
        [HttpGet("GetPurchaseDetailsWithHsnTaxByModelNo/{modelNo}")]
        [ProducesResponseType(typeof(ItemMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPurchaseDetailsWithHsnTaxByModelNo(string modelNo)
        {
            try
            {
                var data = await _itemMasterService.GetPurchaseDetailsWithHsnTaxByModelNo(modelNo);

                if (data == null)
                    return NotFound("Model not found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetByItemType/{itemType}")]
        [ProducesResponseType(typeof(IEnumerable<ItemMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ItemMaster>>> GetItemByItemType(int itemType)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var items = await _itemMasterService.GetItemByItemType(itemType);

                return Ok(items);

            }
            catch { throw; }
        }

        [HttpGet("GetItemsByOEMModel/{id}")]
        [ProducesResponseType(typeof(IEnumerable<ItemMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ItemMaster>>> GetItemsByOEMModel(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var items = await _itemMasterService.GetItemsByOEMModel(id);

                return Ok(items);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
