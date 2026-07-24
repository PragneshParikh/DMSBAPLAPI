using DMS_BAPL_Data.Services.EstimateService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstimateController : ControllerBase
    {
        private readonly IEstimateService _service;

        public EstimateController(IEstimateService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> Create(EstimateCreateViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin || string.IsNullOrWhiteSpace(model.DealerCode))
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        model.DealerCode = tokenDealerCode;
                }

                var id = await _service.CreateAsync(model, userId);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound();

                // Ownership check: non-admins may only view/load estimates
                // belonging to their own dealer.
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode) &&
                        !string.Equals(result.DealerCode, tokenDealerCode, StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] EstimateFilterModel filter)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // Dealer restriction: non-admins are forced to their own dealer,
                // matching the same pattern used in ReportController's report
                // endpoints — never trust a client-supplied dealerCode for
                // non-admin access scoping.
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        filter.DealerCode = tokenDealerCode;
                }

                var result = await _service.GetAllAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EstimateCreateViewModel model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    var existing = await _service.GetByIdAsync(id);
                    if (existing == null) return NotFound();

                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode) &&
                        !string.Equals(existing.DealerCode, tokenDealerCode, StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }
                }

                await _service.UpdateAsync(id, model, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result) return NotFound();
                return Ok("Deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("getNextEstimationNo")]
        public async Task<IActionResult> GetNextEstimationNo()
        {
            try
            {
                var result = await _service.GenerateNextEstimationNoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("job-types")]
        public async Task<IActionResult> GetJobTypes()
        {
            try
            {
                var result = await _service.GetJobTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("estimation-numbers")]
        public async Task<IActionResult> GetEstimationNumbers([FromQuery] string? dealerCode)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode))
                        dealerCode = tokenDealerCode;
                }

                var result = await _service.GetEstimationNumbersAsync(dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet("search-parts")]
        public async Task<IActionResult> SearchParts([FromQuery] string? query, [FromQuery] int maxResults = 20)
        {
            try
            {
                var result = await _service.SearchPartsAsync(query ?? "", maxResults);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("search-labour")]
        public async Task<IActionResult> SearchLabour([FromQuery] string? query, [FromQuery] int maxResults = 20)
        {
            try
            {
                var result = await _service.SearchLabourAsync(query ?? "", maxResults);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadEstimatePdf(int id)
        {
            try
            {
                var estimate = await _service.GetByIdAsync(id);
                if (estimate == null) return NotFound("Estimate not found");

                // Same ownership check pattern as Get/Update — non-admins may only
                // print their own dealer's estimate.
                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
                if (!isAdmin)
                {
                    string tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (!string.IsNullOrEmpty(tokenDealerCode) &&
                        !string.Equals(estimate.DealerCode, tokenDealerCode, StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }
                }

                var pdfBytes = await _service.DownloadEstimatePdfAsync(id);
                if (pdfBytes == null) return NotFound("Estimate not found");

                return File(pdfBytes, "application/pdf", $"Estimate_{estimate.EstimationNo}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}