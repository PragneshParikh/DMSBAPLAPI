using DMS_BAPL_Data.Services.FileService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IFileService filesService, IConfiguration configuration, ILogger<FilesController> logger)
        {
            _fileService = filesService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("generate-upload-url")]
        public async Task<IActionResult> GenerateUploadUrl(string fileName)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // 25.11.2026: clear message instead of an unhandled 500 when Azure Blob storage is not configured
                if (string.IsNullOrWhiteSpace(_configuration["AzureStorage:ConnectionString"]) ||
                    string.IsNullOrWhiteSpace(_configuration["AzureStorage:ContainerName"]))
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                        message = "Photo upload is not configured on the API server: AzureStorage:ConnectionString and AzureStorage:ContainerName are missing (user secrets / App Service settings)."
                    });
                }
                if (string.IsNullOrWhiteSpace(fileName))
                    return BadRequest(new { message = "fileName is required." });

                var result = await _fileService.GenerateUploadUrl(fileName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "generate-upload-url failed for {FileName}", fileName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Upload URL could not be created: " + ex.GetBaseException().Message });
            }
        }
    }
}
