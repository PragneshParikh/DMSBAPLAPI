using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Data.Services.B2BJobCardService;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Data.Services.MaterialTransferService;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System.Text.RegularExpressions;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/material-transfer")]
    [ApiController]
    public class MaterialTransferController : ControllerBase
    {
        private readonly IMaterialTransferService _materialTransferService;
        private readonly IJobCardRepo _jobCardRepo;
        private readonly ILogger<MaterialTransferController> _logger;
        private readonly IPrefixService _prefixService;
        private readonly IPartInventoryService _partInventoryService;
        private readonly IB2BJobCardService _b2bJobCardService;

        public MaterialTransferController(IMaterialTransferService materialTransferService, IJobCardRepo jobCardRepo,
            ILogger<MaterialTransferController> logger, IPrefixService prefixService, IPartInventoryService partInventoryService,
            IB2BJobCardService b2bJobCardService)
        {
            _materialTransferService = materialTransferService;
            _jobCardRepo = jobCardRepo;
            _logger = logger;
            _prefixService = prefixService;
            _b2bJobCardService = b2bJobCardService;
            _partInventoryService = partInventoryService;
        }

        private B2BCaller GetB2BCaller()
        {
            var isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
            return new B2BCaller
            {
                UserId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext) ?? "",
                IsAdmin = isAdmin,
                DealerCode = isAdmin ? null : GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)?.Trim()
            };
        }

        private async Task<(int? StatusCode, string? Message, string? DealerCode)> ValidateB2BSourceAsync(
            Guid b2bJobCardId,
            string? requestedDealerCode)
        {
            try
            {
                var jobCard = await _b2bJobCardService.GetAsync(GetB2BCaller(), b2bJobCardId, HttpContext.RequestAborted);
                if (string.IsNullOrWhiteSpace(jobCard.DealerCode))
                    return (StatusCodes.Status400BadRequest, "The B2B job card has no mapped DMS dealer code.", null);

                if (!string.IsNullOrWhiteSpace(requestedDealerCode)
                    && !string.Equals(requestedDealerCode.Trim(), jobCard.DealerCode.Trim(), StringComparison.OrdinalIgnoreCase))
                    return (StatusCodes.Status403Forbidden, "The selected B2B job card belongs to a different dealer.", null);

                return (null, null, jobCard.DealerCode);
            }
            catch (B2BNotFoundException)
            {
                return (StatusCodes.Status404NotFound, "B2B job card not found.", null);
            }
            catch (B2BValidationException ex)
            {
                return (StatusCodes.Status400BadRequest, ex.Message, null);
            }
        }

        private static bool HasValidSource(List<MaterialTransferViewModel> items)
        {
            if (items.Count == 0)
                return false;

            var first = items[0];
            return items.All(item =>
                item.B2BJobCardId == first.B2BJobCardId
                && (first.B2BJobCardId.HasValue
                    ? !item.JobId.HasValue
                    : item.JobId.HasValue));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MaterialTransfer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var materilList = await _materialTransferService.Get();

                return Ok(materilList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching the records : {ex.Message}");
                throw;
            }
        }

        [HttpGet("issue-id")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetIssueId()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var issueId = await _materialTransferService.GetIssueIdAsync();
                return Ok(issueId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting issue ID.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{JobId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterialTransferListByJobId(int JobId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var materialTransfer = await _materialTransferService.GetMeterialByJobId(JobId);

                return Ok(materialTransfer);

            }

            catch (Exception ex)
            {
                _logger.LogError($"Error while featching data by Job id: {ex.Message}");
                throw;
            }
        }

        [HttpGet("b2b/{b2bJobCardId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<MaterialTransferViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MaterialTransferViewModel>>> GetMaterialTransferByB2BJobCardId(Guid b2bJobCardId)
        {
            string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authorized");

            var validation = await ValidateB2BSourceAsync(b2bJobCardId, null);
            if (validation.StatusCode.HasValue)
                return StatusCode(validation.StatusCode.Value, new { message = validation.Message });

            var materials = await _materialTransferService.GetMaterialTransferByB2BJobCardId(b2bJobCardId);
            return Ok(materials);
        }

        [HttpGet("GetByDealerPaged")]
        [ProducesResponseType(typeof(PagedResponse<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<object>>> GetMaterialTransferDetailByDealer(
            [FromQuery] string? searchTerm,
            [FromQuery] string? dealerCode,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                bool isSuperAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);

                // Only a Super Admin's request is allowed to actually search "all dealers".
                // A regular user's dealerCode is always forced server-side, ignoring
                // whatever value the client sent.
                string? effectiveDealerCode = isSuperAdmin
                    ? dealerCode
                    : GetUserInfoFromToken.GetDealerCode(HttpContext);

                var materials = await _materialTransferService.GetMaterialTransferDetailsByDealer(searchTerm, effectiveDealerCode, pageIndex, pageSize, fromDate, toDate);

                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in feathing material details by dealer : {ex.Message}");
                throw;
            }
        }

        [HttpGet("download")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> downloadExcel()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                bool isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);

                string? dealerCode = null;

                // Normal users only
                if (!isAdmin)
                    dealerCode = GetUserInfoFromToken.GetDealerCode(HttpContext);

                var file = await _materialTransferService.downloadMaterialExcel(dealerCode);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "MaterialTransfer.xlsx"
                );
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

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> InsertMaterialTrasferList(List<MaterialTransferViewModel> materialTransferViewModels)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                if (materialTransferViewModels == null || !HasValidSource(materialTransferViewModels))
                    return BadRequest(new { message = "Material transfer rows must reference one valid B2C or B2B job card." });

                if (materialTransferViewModels[0].B2BJobCardId is Guid b2bJobCardId)
                {
                    var validation = await ValidateB2BSourceAsync(b2bJobCardId, materialTransferViewModels[0].DealerCode);
                    if (validation.StatusCode.HasValue)
                        return StatusCode(validation.StatusCode.Value, new { message = validation.Message });

                    foreach (var item in materialTransferViewModels)
                        item.DealerCode = validation.DealerCode;
                }

                var material = await _materialTransferService.InsertMaterials(materialTransferViewModels);

                if (material > 0 && materialTransferViewModels[0].Id <= 0)
                {
                    if (materialTransferViewModels[0].JobId is int jobId && jobId > 0)
                        await _jobCardRepo.UpdateMaterialTransferStatus(jobId, true);
                    await _prefixService.UpdateNextNumberByDealerByModule(materialTransferViewModels[0].DealerCode, "material_transfer");
                }

                return Ok(material);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.StartsWith("Insufficient stock.", StringComparison.Ordinal))
            {
                _logger.LogWarning(ex, "Material Transfer rejected because stock is insufficient.");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while inserting Material Transfer rows.");
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateMaterialsDetails(List<MaterialTransferViewModel> materialTransferViewModels)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");                
                if (materialTransferViewModels == null || !HasValidSource(materialTransferViewModels))
                    return BadRequest(new { message = "Material transfer rows must reference one valid B2C or B2B job card." });

                if (materialTransferViewModels[0].B2BJobCardId is Guid b2bJobCardId)
                {
                    var validation = await ValidateB2BSourceAsync(b2bJobCardId, materialTransferViewModels[0].DealerCode);
                    if (validation.StatusCode.HasValue)
                        return StatusCode(validation.StatusCode.Value, new { message = validation.Message });

                    foreach (var item in materialTransferViewModels)
                        item.DealerCode = validation.DealerCode;
                }

                var materials = await _materialTransferService.UpdateMaterialDetails(materialTransferViewModels);

                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating material detail: {ex.Message}");
                throw;
            }
        }

        [HttpDelete]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<int>> DeleteMaterials([FromBody] List<MaterialTransferViewModel> materialTransferViewModels)
        public async Task<ActionResult<int>> DeleteMaterials([FromBody] List<int> Ids)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var material = await _materialTransferService.DeleteMaterials(Ids);

                return Ok(material);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete("by-job/{jobId}")]
        [ProducesResponseType(typeof(MaterialTransferDeleteResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MaterialTransferDeleteResultViewModel>> DeleteMaterialTransferByJobId(int jobId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                bool isSuperAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);

                var result = await _materialTransferService.DeleteMaterialsByJobId(jobId, isSuperAdmin, userId);

                if (result.DeletedCount > 0)
                {
                    await _jobCardRepo.UpdateMaterialTransferStatus(jobId, false);
                    await _jobCardRepo.MarkJobCardAsDeleted(jobId, userId);
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting material transfer for job {jobId}: {ex.Message}");
                throw;
            }
        }
    }
}
