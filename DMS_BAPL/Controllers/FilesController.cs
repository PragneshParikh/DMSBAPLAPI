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

        public FilesController(IFileService filesService)
        {
            _fileService = filesService;
        }

        [HttpGet("generate-upload-url")]
        public async Task<IActionResult> GenerateUploadUrl(string fileName)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var result = await _fileService.GenerateUploadUrl(fileName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
