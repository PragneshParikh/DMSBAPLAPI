using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.NewsBulletinAttachmentService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/news-bulletin-attachment")]
    [ApiController]
    public class CircularAttachmentController : ControllerBase
    {
        private readonly ICircularAttachmentService _circularAttachmentService;
        private readonly ILogger<CircularAttachmentController> _logger;
        public CircularAttachmentController(ICircularAttachmentService circularAttachmentService, ILogger<CircularAttachmentController> logger)
        {
            _circularAttachmentService = circularAttachmentService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CircularMasterAttachment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CircularMasterAttachment>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _circularAttachmentService.Get();

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching News Bulletin Attachment : {ex.Message}");
                throw;
            }
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(CircularMasterAttachment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CircularMasterAttachment>> GetById(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _circularAttachmentService.GetById(Id);

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errror while featching record by Id : {ex.Message}");
                throw;
            }
        }

        [HttpGet("GetByCircularId/{Id}")]
        [ProducesResponseType(typeof(IEnumerable<CircularMasterAttachment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CircularMasterAttachment>>> GetByCircularId(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _circularAttachmentService.GetByCircularId(Id);

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
        public async Task<ActionResult<bool>> Insert(List<CircularMasterAttachment> circularMasterAttachment)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var attachment = await _circularAttachmentService.Insert(circularMasterAttachment);

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
