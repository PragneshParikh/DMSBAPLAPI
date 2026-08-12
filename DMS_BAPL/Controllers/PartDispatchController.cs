using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.PartDispatchService;
using DMS_BAPL_Data.Services.PartsInwardService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartDispatchController : ControllerBase
    {
        private readonly IPartDispatchService _service;
        private readonly IPartInwardService _partInwardService;

        public PartDispatchController(IPartDispatchService service, IPartInwardService partInwardService)
        {
            _service = service;
            _partInwardService = partInwardService;
        }

        /// <summary>
        /// Get all part dispatch records
        /// </summary>
        [HttpGet("~/api/erppartdispatch/dispatch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDispatch()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get single record by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new record. Id is auto-generated — do not send it.
        /// </summary>
        //[HttpPost("~/api/erppartdispatch/dispatch")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //public async Task<IActionResult> Create([FromBody] PartDispatchCreateViewModel model)
        //{
        //    if (model == null)
        //        return BadRequest(new { success = false, message = "Invalid data" });

        //    try
        //    {
        //        string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
        //        if (string.IsNullOrEmpty(userId))
        //            return Unauthorized("User not authorized");

        //        var entity = new DmsPartDispatch
        //        {
        //            InvoiceDate = model.InvoiceDate,
        //            InvoiceNo = model.InvoiceNo,
        //            PartNo = model.PartNo,
        //            ItemIdno = model.ItemIdno,
        //            ItemHsncode = model.ItemHsncode,
        //            ItemRate = model.ItemRate,
        //            ItemMrp = model.ItemMrp,
        //            ItemQty = model.ItemQty,
        //            Sgst = model.Sgst,
        //            Cgst = model.Cgst,
        //            Igst = model.Igst,
        //            Ugst = model.Ugst,
        //            ItemDisc = model.ItemDisc,
        //            DiscountType = model.DiscountType,
        //            LocCode = model.LocCode,
        //            VendorIdno = model.VendorIdno,
        //            DealerCode = model.DealerCode
        //        };

        //        var result = await _service.CreateAsync(entity, userId);
        //        return Ok(new { success = true, data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        //    }
        //}
        /// <summary>
        /// Create one or more records. Send a plain JSON array, e.g. [ {...}, {...} ].
        /// Id is auto-generated — do not send it.
        /// </summary>
        //[HttpPost("~/api/erppartdispatch/dispatch")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //public async Task<IActionResult> Create([FromBody] List<PartDispatchCreateViewModel> models)
        //{
        //    if (models == null || models.Count == 0)
        //        return BadRequest(new { success = false, message = "Invalid data" });

        //    try
        //    {
        //        string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
        //        if (string.IsNullOrEmpty(userId))
        //            return Unauthorized("User not authorized");

        //        var entities = models.Select(model => new DmsPartDispatch
        //        {
        //            InvoiceDate = model.InvoiceDate,
        //            InvoiceNo = model.InvoiceNo,
        //            PartNo = model.PartNo,
        //            ItemIdno = model.ItemIdno,
        //            ItemHsncode = model.ItemHsncode,
        //            ItemRate = model.ItemRate,
        //            ItemMrp = model.ItemMrp,
        //            ItemQty = model.ItemQty,
        //            Sgst = model.Sgst,
        //            Cgst = model.Cgst,
        //            Igst = model.Igst,
        //            Ugst = model.Ugst,
        //            ItemDisc = model.ItemDisc,
        //            DiscountType = model.DiscountType,
        //            LocCode = model.LocCode,
        //            VendorIdno = model.VendorIdno,
        //            DealerCode = model.DealerCode
        //        }).ToList();

        //        var results = await _service.CreateBulkAsync(entities, userId);
        //        return Ok(new { success = true, data = results });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost("~/api/erppartdispatch/dispatch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] PartDispatchCreateViewModel model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var entity = new DmsPartDispatch
                {
                    InvoiceDate = model.InvoiceDate,
                    InvoiceNo = model.InvoiceNo,
                    PartNo = model.PartNo,
                    ItemIdno = model.ItemIdno,
                    ItemHsncode = model.ItemHsncode,
                    ItemRate = model.ItemRate,
                    ItemMrp = model.ItemMrp,
                    ItemQty = model.ItemQty,
                    Sgst = model.Sgst,
                    Cgst = model.Cgst,
                    Igst = model.Igst,
                    Ugst = model.Ugst,
                    ItemDisc = model.ItemDisc,
                    DiscountType = model.DiscountType,
                    LocCode = model.LocCode,
                    VendorIdno = model.VendorIdno,
                    DealerCode = model.DealerCode
                };

                var result = await _service.CreateAsync(entity, userId);

                if (!string.IsNullOrEmpty(result.PartNo) && result.PartNo.ToUpper().Contains("EW"))
                {
                    await _partInwardService.CreateFromDispatchAsync(result, userId);
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing record
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] DmsPartDispatch model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                model.Id = id;
                var result = await _service.UpdateAsync(model, userId);

                if (result == null)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a record
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Bulk import from Excel file
        /// </summary>
        [HttpPost("import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded" });

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                using var stream = file.OpenReadStream();
                var count = await _service.ImportFromExcelAsync(stream, userId);

                return Ok(new { success = true, message = $"{count} records imported successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }
    }
}