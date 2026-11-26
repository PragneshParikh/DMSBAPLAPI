using DMS_BAPL_Api.Helpers;
using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.FFIRRepo;
using DMS_BAPL_Data.Services.B2BJobCardService;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FFIRController : ControllerBase
    {
        private readonly IFFIRRepo _ffirRepo;
        private readonly IPrefixService _prefixService;
        private readonly ILogger<FFIRController> _logger;
        private readonly IB2BJobCardService _b2bJobCardService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public FFIRController(
            IFFIRRepo ffirRepo,
            IPrefixService prefixService,
            ILogger<FFIRController> logger,
            IB2BJobCardService b2bJobCardService,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _ffirRepo = ffirRepo;
            _prefixService = prefixService;
            _logger = logger;
            _b2bJobCardService = b2bJobCardService;
            _configuration = configuration;
            _environment = environment;
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

        [HttpGet("GetPartDropdownlist")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartDropdownlist()
        {
            var partDropdownlist = await _ffirRepo.GetPartDropdownlist();
            return Ok(partDropdownlist);
        }
        [HttpGet("GetComplaintCodeList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComplaintCodeList()
        {
            var complaintCodeList = await _ffirRepo.GetComplaintCodeList();
            return Ok(complaintCodeList);
        }

        [HttpGet("GetJobCardHistory/{chassisNo}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobCardHistory(string chassisNo)
        {
            try
            {
                var jobCardHistory = await _ffirRepo.GetJobCardHistory(chassisNo);
                return Ok(jobCardHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPost("InsertFFIR")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertFFIR([FromBody] FFIRViewModel model)
        {
            try
            {
                
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var validationMessage = ValidateRequiredFields(model);
                if (validationMessage != null)
                    return BadRequest(new { message = validationMessage });

                if (model.B2BJobCardId.HasValue)
                {
                    if (model.JobCardHeaderId.HasValue || model.JobCardCustomerId.HasValue)
                        return BadRequest(new { success = false, message = "FFIR cannot reference both a B2B and DMS job card." });

                    var b2bJobCard = await _b2bJobCardService.GetAsync(
                        GetB2BCaller(), model.B2BJobCardId.Value, HttpContext.RequestAborted);

                    if (!b2bJobCard.CanEdit)
                        return BadRequest(new { success = false, message = "FFIR cannot be created for a closed or cancelled B2B job card." });
                    if (string.IsNullOrWhiteSpace(b2bJobCard.DealerCode))
                        return BadRequest(new { success = false, message = "The B2B job card has no mapped DMS dealer code." });

                    model.DealerCode = b2bJobCard.DealerCode;
                    model.FFIRChassisNo = b2bJobCard.Vehicle.ChassisNo ?? model.FFIRChassisNo;
                }
                else if (!model.JobCardHeaderId.HasValue || !model.JobCardCustomerId.HasValue)
                {
                    return BadRequest(new { success = false, message = "A DMS job card is required for this FFIR." });
                }

                var result = await _ffirRepo.InsertFFIRAsync(model,userId);
                if (result > 0)
                {
                    await _prefixService.UpdateNextNumberByDealerByModule(model.DealerCode, "ffir_prefix");
                    return Ok(new
                    {
                        success = true,
                        message = StringConstants.FFIRInsert,
                        id = result
                    });
                }
                else
                {
                    _logger.LogError("Failed to insert JobCard.");
                    return StatusCode(500, "An error occurred while saving job card details.");
                }

                
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("GetFFIRDetailListing")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFFIRDetailListing(string dealerCode, string? search)
        {
            if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
            {
                dealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)?.Trim();
                if (string.IsNullOrWhiteSpace(dealerCode))
                    return Unauthorized("User is not associated with a dealer.");
            }

            var ffirDetailListing = await _ffirRepo.GetFFIRDetailListing(dealerCode, search);
            return Ok(ffirDetailListing);

        }

        [HttpGet("GetFFIRById/{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFFIRById(int id)
        {
            var result = await _ffirRepo.GetFFIRById(id);
            if (result == null)
                return NotFound();

            if (result.B2BJobCardId is Guid b2bJobCardId)
            {
                try
                {
                    await _b2bJobCardService.GetAsync(
                        GetB2BCaller(), b2bJobCardId, HttpContext.RequestAborted);
                }
                catch (B2BNotFoundException)
                {
                    return NotFound();
                }
            }
            return Ok(result);
        }

        [HttpGet("GetFFIRAttachments/{id:int}")]
        public async Task<IActionResult> GetFFIRAttachments(int id, CancellationToken ct)
        {
            var (ffir, accessError) = await GetAccessibleFFIRAsync(id, ct);
            if (accessError != null) return accessError;

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(ct);
                await using var command = new SqlCommand(@"
                    SELECT Id, FileName, FileUrl, ContentType, FileSize, MediaType
                    FROM dbo.FFIRAttachment
                    WHERE FFIRId = @ffirId AND IsDeleted = 0
                    ORDER BY Id", connection);
                command.Parameters.Add("@ffirId", SqlDbType.Int).Value = ffir!.Id;

                var attachments = new List<object>();
                await using var reader = await command.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    attachments.Add(new
                    {
                        id = reader.GetInt32(0),
                        fileName = reader.GetString(1),
                        url = reader.GetString(2),
                        contentType = reader.GetString(3),
                        fileSize = reader.GetInt64(4),
                        mediaType = reader.GetString(5)
                    });
                }

                return Ok(attachments);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Table dbo.FFIRAttachment does not exist. Run 20261126_AddFFIRAttachments.sql on the DMS database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get attachments for FFIR {FFIRId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load FFIR attachments." });
            }
        }

        [HttpPost("UploadFFIRAttachment/{id:int}")]
        [RequestSizeLimit(105_000_000)]
        public async Task<IActionResult> UploadFFIRAttachment(
            int id,
            IFormFile file,
            CancellationToken ct)
        {
            var (ffir, accessError) = await GetAccessibleFFIRAsync(id, ct);
            if (accessError != null) return accessError;

            FFIRAttachmentStorage.MediaInfo media;
            try
            {
                media = await FFIRAttachmentStorage.ValidateAsync(file, ct);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            FFIRAttachmentStorage.Saved? saved = null;
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(ct);
                await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);

                await using (var countCommand = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM dbo.FFIRAttachment WITH (UPDLOCK, HOLDLOCK)
                    WHERE FFIRId = @ffirId AND MediaType = @mediaType AND IsDeleted = 0", connection, transaction))
                {
                    countCommand.Parameters.Add("@ffirId", SqlDbType.Int).Value = ffir!.Id;
                    countCommand.Parameters.Add("@mediaType", SqlDbType.NVarChar, 10).Value = media.MediaType;
                    var count = (int)(await countCommand.ExecuteScalarAsync(ct) ?? 0);
                    var limit = media.MediaType == "Image" ? 10 : 5;
                    if (count >= limit)
                        return BadRequest(new { message = $"A maximum of {limit} {media.MediaType.ToLowerInvariant()} attachments is allowed per FFIR." });
                }

                saved = await FFIRAttachmentStorage.SaveAsync(
                    file, media, ffir!.Id, Request, _configuration, _environment.ContentRootPath, ct);

                await using var insertCommand = new SqlCommand(@"
                    INSERT INTO dbo.FFIRAttachment
                        (FFIRId, DealerCode, FileName, FileUrl, ContentType, FileSize, MediaType, CreatedBy)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ffirId, @dealerCode, @fileName, @fileUrl, @contentType, @fileSize, @mediaType, @createdBy)",
                    connection, transaction);
                insertCommand.Parameters.Add("@ffirId", SqlDbType.Int).Value = ffir.Id;
                insertCommand.Parameters.Add("@dealerCode", SqlDbType.NVarChar, 50).Value = (object?)ffir.DealerCode ?? DBNull.Value;
                insertCommand.Parameters.Add("@fileName", SqlDbType.NVarChar, 260).Value = Truncate(saved.FileName, 260) ?? "attachment";
                insertCommand.Parameters.Add("@fileUrl", SqlDbType.NVarChar, 1000).Value = saved.Url;
                insertCommand.Parameters.Add("@contentType", SqlDbType.NVarChar, 100).Value = saved.ContentType;
                insertCommand.Parameters.Add("@fileSize", SqlDbType.BigInt).Value = saved.Size;
                insertCommand.Parameters.Add("@mediaType", SqlDbType.NVarChar, 10).Value = saved.MediaType;
                insertCommand.Parameters.Add("@createdBy", SqlDbType.NVarChar, 100).Value =
                    Truncate(GetUserInfoFromToken.GetUserIdFromToken(HttpContext), 100) ?? "";
                var attachmentId = (int)(await insertCommand.ExecuteScalarAsync(ct) ?? 0);
                await transaction.CommitAsync(ct);

                return CreatedAtAction(nameof(GetFFIRAttachments), new { id }, new
                {
                    id = attachmentId,
                    fileName = saved.FileName,
                    url = saved.Url,
                    contentType = saved.ContentType,
                    fileSize = saved.Size,
                    mediaType = saved.MediaType
                });
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                DeleteSavedFile(saved);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Table dbo.FFIRAttachment does not exist. Run 20261126_AddFFIRAttachments.sql on the DMS database."
                });
            }
            catch (Exception ex)
            {
                DeleteSavedFile(saved);
                _logger.LogError(ex, "Failed to upload attachment for FFIR {FFIRId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save the FFIR attachment." });
            }
        }

        private async Task<(FFIRViewModel? Ffir, IActionResult? Error)> GetAccessibleFFIRAsync(int id, CancellationToken ct)
        {
            if (HttpContext.User.Identity?.IsAuthenticated != true)
                return (null, Unauthorized(new { message = "User not authorized" }));

            var ffir = await _ffirRepo.GetFFIRById(id);
            if (ffir == null)
                return (null, NotFound(new { message = "FFIR not found." }));

            if (ffir.B2BJobCardId is Guid b2bJobCardId)
            {
                try
                {
                    await _b2bJobCardService.GetAsync(GetB2BCaller(), b2bJobCardId, ct);
                }
                catch (B2BNotFoundException)
                {
                    return (null, NotFound(new { message = "FFIR not found." }));
                }
                catch (B2BValidationException)
                {
                    return (null, NotFound(new { message = "FFIR not found." }));
                }
            }
            else if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
            {
                var ownDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)?.Trim();
                if (string.IsNullOrWhiteSpace(ownDealerCode)
                    || !string.Equals(ownDealerCode, ffir.DealerCode?.Trim(), StringComparison.OrdinalIgnoreCase))
                    return (null, NotFound(new { message = "FFIR not found." }));
            }

            return (ffir, null);
        }

        private SqlConnection CreateConnection() =>
            new(_configuration.GetConnectionString("DefaultConnection"));

        private void DeleteSavedFile(FFIRAttachmentStorage.Saved? saved)
        {
            if (saved == null) return;
            try
            {
                if (System.IO.File.Exists(saved.Path))
                    System.IO.File.Delete(saved.Path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove unattached FFIR file {FilePath}", saved.Path);
            }
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            return value.Length > maxLength ? value[..maxLength] : value;
        }

        private static string? ValidateRequiredFields(FFIRViewModel model)
        {
            var observation = model.DetailObservation;
            if (string.IsNullOrWhiteSpace(model.PurposeOfCIR)
                || string.IsNullOrWhiteSpace(model.ReportTitle)
                || string.IsNullOrWhiteSpace(model.ReportPreparedBy)
                || string.IsNullOrWhiteSpace(model.TypeOfRoadSurface)
                || string.IsNullOrWhiteSpace(observation?.ObservationFailedParts)
                || string.IsNullOrWhiteSpace(observation?.RootCauseofFailure)
                || string.IsNullOrWhiteSpace(observation?.CorrectiveAction)
                || string.IsNullOrWhiteSpace(observation?.ResolutionComplaint)
                || string.IsNullOrWhiteSpace(observation?.PresentStatusofVehicle))
            {
                return "Purpose of FFIR, Report Title, Report Prepared by, Type of Road Surface, and all required Detailed Observation fields must be completed.";
            }

            return null;
        }

        [HttpPut("UpdateFFIR/{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFFIR(int id, [FromBody] FFIRViewModel model)
        {
            try
            {
                var existing = await _ffirRepo.GetFFIRById(id);
                if (existing == null)
                    return NotFound();

                if (existing.B2BJobCardId.HasValue)
                {
                    if (model.B2BJobCardId != existing.B2BJobCardId)
                        return BadRequest("The B2B job card reference cannot be changed.");

                    var b2bJobCard = await _b2bJobCardService.GetAsync(
                        GetB2BCaller(), existing.B2BJobCardId.Value, HttpContext.RequestAborted);

                    if (!b2bJobCard.CanEdit)
                        return BadRequest("FFIR cannot be updated for a closed or cancelled B2B job card.");
                    if (string.IsNullOrWhiteSpace(b2bJobCard.DealerCode))
                        return BadRequest("The B2B job card has no mapped DMS dealer code.");

                    model.JobCardHeaderId = null;
                    model.JobCardCustomerId = null;
                    model.DealerCode = b2bJobCard.DealerCode;
                }
                else if (model.B2BJobCardId.HasValue)
                {
                    return BadRequest("A DMS FFIR cannot be changed to reference a B2B job card.");
                }

                var validationMessage = ValidateRequiredFields(model);
                if (validationMessage != null)
                    return BadRequest(new { message = validationMessage });

                var result = await _ffirRepo.UpdateFFIRAsync(id, model);

                if (!result)
                    return NotFound();

                return Ok(new
                {
                    message = "FFIR Updated Successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteFFIR/{id}/{role}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFFIR(int id, string role)
        {
            try
            {
                if (role != "SuperAdmin")
                    return Unauthorized("Only Super Admin can delete");

                var result = await _ffirRepo.DeleteFFIR(id, role);

                if (result > 0)
                    return Ok(new { message = "Deleted Successfully" });

                return NotFound("FFIR not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteFFIR");
                return StatusCode(500, "An error occurred while deleting the FFIR.");
            }

        }

    }
}
