using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.PartDispWarrantyService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartDispWarrantyController : ControllerBase
    {
        private readonly IPartDispWarrantyService _service;

        public PartDispWarrantyController(IPartDispWarrantyService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all dispatch/warranty records
        /// </summary>
        [HttpGet("~/api/dispatch/srd")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSrd()
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
        /// Create one or more records. Accepts { "data": [ {...}, {...} ] } — matches
        /// the exact payload shape used in Postman/live integration testing.
        /// </summary>
        [HttpPost("~/api/dispatch/srd")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] PartDispWarrantyBulkCreateViewModel payload)
        {
            if (payload == null || payload.Data == null || payload.Data.Count == 0)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var results = new List<ZDmsPartDispWarranty>();

                foreach (var model in payload.Data)
                {
                    var entity = new ZDmsPartDispWarranty
                    {
                        Invoicedate = model.Invoicedate,
                        Invoiceno = model.Invoiceno,
                        Invoicetype = model.Invoicetype,
                        Chassisnumber = model.Chassisnumber,
                        Itemcode = model.Itemcode,
                        Serialno = model.Serialno,
                        Dealercode = model.Dealercode,
                        Devicetype = model.Devicetype,
                        Itemqty = model.Itemqty,
                        Lotno = model.Lotno,
                        Mfgdate = model.Mfgdate,
                        Invoiceitemcode = model.Invoiceitemcode,
                        Lineno = model.Lineno,
                        InvoiceAmt = model.InvoiceAmt
                        // REMOVED: Vendorid = model.Vendorid
                    };

                    var created = await _service.CreateAsync(entity, userId);
                    results.Add(created);
                }

                return Ok(new { success = true, data = results });
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
        public async Task<IActionResult> Update(int id, [FromBody] ZDmsPartDispWarranty model)
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

        [HttpGet("~/api/dispatch/serials-by-itemcode/{itemCode}")]
        public async Task<IActionResult> GetSerialsByItemCode(string itemCode, [FromQuery] int? excludeInvoiceId)
        {
            var result = await _service.GetSerialNosByItemCodeAsync(itemCode, excludeInvoiceId);
            return Ok(new { data = result });
        }


        [HttpPost("~/api/dispatch/by-itemcodes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByItemCodes([FromBody] List<string> itemCodes)
        {
            try
            {
                var result = await _service.GetByItemCodesAsync(itemCodes);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("~/api/dispatch/by-serialno/{serialNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBySerialNo(string serialNo)
        {
            try
            {
                var result = await _service.GetBySerialNoAsync(serialNo);
                if (result == null)
                    return NotFound(new { success = false, message = "No dispatch record found for this serial no." });

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }
    }
}