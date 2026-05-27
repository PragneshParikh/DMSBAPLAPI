using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.NewsBulletinAttachmentService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/news-bulletin-attachment")]
    [ApiController]
    public class NewsBulletinAttachmentController : ControllerBase
    {
        private readonly INewsBulletinAttachmentService _newsBulletinAttachmentService;
        private readonly ILogger<NewsBulletinAttachmentController> _logger;
        public NewsBulletinAttachmentController(INewsBulletinAttachmentService newsBulletinAttachmentService, ILogger<NewsBulletinAttachmentController> logger)
        {
            _newsBulletinAttachmentService = newsBulletinAttachmentService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NewsBulletinMasterAttachment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NewsBulletinMasterAttachment>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _newsBulletinAttachmentService.Get();

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching News Bulletin Attachment : {ex.Message}");
                throw;
            }
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(NewsBulletinMasterAttachment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NewsBulletinMasterAttachment>> GetById(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _newsBulletinAttachmentService.GetById(Id);

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errror while featching record by Id : {ex.Message}");
                throw;
            }
        }

        [HttpGet("GetByNewsBulletinId/{Id}")]
        [ProducesResponseType(typeof(IEnumerable<NewsBulletinMasterAttachment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NewsBulletinMasterAttachment>>> GetByNewsBulletinId(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _newsBulletinAttachmentService.GetByNewsBulletinId(Id);

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching records by bulleting Id :{ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Insert(List<NewsBulletinMasterAttachment> newsBulletinMasterAttachment)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _newsBulletinAttachmentService.Insert(newsBulletinMasterAttachment);

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting news bulletin attachment : {ex.Message}");
                throw;
            }
        }

        //[HttpDelete]
        //[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<bool>> Delete(List<NewsBulletinMasterAttachment> newsBulletinMasterAttachments)
        //{
        //    try
        //    {
        //        string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

        //        if (string.IsNullOrEmpty(userId))
        //            return Unauthorized("User not authorized");

        //        var attachment = await _newsBulletinAttachmentService.Delete(newsBulletinMasterAttachments);

        //        return Ok(attachment);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error while deleting attachment : {ex.Message}");
        //        throw;
        //    }
        //}

    }
}
