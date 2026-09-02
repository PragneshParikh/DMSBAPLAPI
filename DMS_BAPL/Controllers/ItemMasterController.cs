using DMS_BAPL_Data;
using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Imports item master (spares/parts) data from an uploaded Excel (.xlsx) file.
        /// Rows are matched by Part Code (Itemcode): an existing item is updated, a new
        /// Part Code is inserted. The sheet's first row must be a header row — column
        /// order doesn't matter as long as the header text matches what
        /// ItemMasterService.ImportItemExcelAsync expects (see that method for the
        /// full list of recognized column aliases).
        /// </summary>
        /// <param name="file">Excel file (.xlsx) containing item master rows</param>
        /// <returns>Import summary: counts of inserted/updated/failed rows, plus any row-level errors</returns>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ItemImportResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportItemExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                var extension = Path.GetExtension(file.FileName);
                if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only .xlsx files are supported.");

                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _itemMasterService.ImportItemExcelAsync(file, userId);

                return Ok(new
                {
                    message = "Item data imported successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                // Bad file / missing or unrecognized header — client error, not a server fault.
                return BadRequest(ex.Message);
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

        [HttpGet("GetItemsWithHsnTaxGroupId")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetItemsWithHsnTaxGroupId([FromQuery] int? groupId, string? dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var items = await _itemMasterService.GetItemsWithHSNTaxGroupId(groupId, dealerCode);

                return Ok(items);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut("UpdateByItemCode")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Object>> UpdateByItemCode([FromBody] insertItemMasterViewModel insertItemMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var item = await _itemMasterService.UpdateByItemCode(userId, insertItemMasterViewModel);

                if (item == null)
                    return NotFound("Item not found with the provided item code.");

                return Ok(new
                {
                    message = "Item updated sucessfully.",
                    data = item
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemMaster item)
        {
            try
            {
                item.Id = id;
                var _item = await _itemMasterService.UpdateItemAsync(item);
                if (_item == null)
                    return NotFound($"Item with id {id} not found.");

                return Ok(_item);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
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

        [HttpGet("GetItemsByLocation")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetItemsByLocation(string dealerLocation, string customerLocation)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var items = await _itemMasterService.GetItemsByLocation(dealerLocation, customerLocation);

                return Ok(items);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetItemModelist")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetItemModelist()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var items = await _itemMasterService.GetItemModelist();
                return Ok(items);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}