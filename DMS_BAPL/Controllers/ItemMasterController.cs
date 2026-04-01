using DMS_BAPL_Data;
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
        public async Task<IActionResult> GetAllItems([FromQuery] int? grpidno, string? search)
        {
            var items = await _itemMasterService.GetAllItemMastersAsync(grpidno, search);
            return Ok(items);


        }

        // PUT api/itemMaster/{id}
        //[HttpPut("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemMaster item)
        {
            item.Id = id;
            await _itemMasterService.UpdateItemAsync(item);
            return Ok("Item updated successfully");
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {
            var file = await _itemMasterService.DownloadItemMasterExcel();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ItemMasterList.xlsx"
            );
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
    }
}
