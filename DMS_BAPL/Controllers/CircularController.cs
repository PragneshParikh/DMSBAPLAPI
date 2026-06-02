using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.NewsBulletinAttachmentService;
using DMS_BAPL_Data.Services.NewsBulletinService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/news-bulletin")]
    [ApiController]
    public class CircularController : ControllerBase
    {
        private readonly ICircularService _circularService;
        private readonly ICircularAttachmentService _circularAttachmentService;
        private readonly ILogger<CircularController> _logger;
        public CircularController(ICircularService circularService, ICircularAttachmentService circularAttachmentService, ILogger<CircularController> logger)
        {
            _circularService = circularService;
            _circularAttachmentService = circularAttachmentService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<ActionResult<object>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var news = await _circularService.Get();

                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching the news bulletin records : {ex.Message}");
                throw;
            }
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(CircularMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetById(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var newsBulletin = await _circularService.GetById(Id);

                return Ok(newsBulletin);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching records by Id : {ex.Message}");
                throw;
            }
        }

        //[HttpGet("GetByDate")]
        //[ProducesResponseType(typeof(NewsBulletinMasterViewModel), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<NewsBulletinFileViewModel>> GetByDate([FromQuery] string date)
        //{
        //    try
        //    {
        //        string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

        //        if (string.IsNullOrEmpty(userId))
        //            return Unauthorized("User not authorized");

        //        var news = await _newsBulletinService.GetByDate(Convert.ToDateTime(date));

        //        return Ok(news);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error while featching recodrs by date : {ex.Message}");
        //        throw;
        //    }
        //}

        [HttpPost]
        [ProducesResponseType(typeof(CircularMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create(CircularMasterViewModel circularMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var circularId = await _circularService.Create(circularMasterViewModel);

                if (circularId > 0)
                {
                    var circularAttachmentList = new List<CircularMasterAttachment>();
                    foreach (var item in circularMasterViewModel.Files)
                    {
                        var circularAtt = new CircularMasterAttachment
                        {
                            CircularId = circularId,
                            FileData = item.FileData,
                            FileName = item.FileName,
                            ContentType = item.ContentType,
                            CreatedBy = circularMasterViewModel.CreatedBy,
                            CreatedDate = circularMasterViewModel.CreatedDate,

                        };

                        circularAttachmentList.Add(circularAtt);
                    }

                    bool result = await _circularAttachmentService.Insert(circularAttachmentList);
                }

                return Ok(circularId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting circular : {ex.Message}");
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(CircularMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(CircularMasterViewModel circularMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var circularId = await _circularService.Update(circularMasterViewModel);

                if (circularId > 0)
                {
                    var newAttachment = new List<CircularMasterAttachment>();
                    var removedAttachmentIds = new List<int>();

                    foreach (var item in circularMasterViewModel.Files)
                    {
                        if (item.Status == "Added")
                        {
                            newAttachment.Add(new CircularMasterAttachment
                            {
                                CircularId = circularId,
                                FileData = item.FileData,
                                FileName = item.FileName,
                                ContentType = item.ContentType,
                                CreatedBy = userId,
                                CreatedDate = DateTime.Now,
                            });
                        }
                        else if (item.Status == "Deleted")
                        {
                            removedAttachmentIds.Add(item.AttachmentId);
                        }
                    }

                    if (newAttachment.Count > 0)
                        await _circularAttachmentService.Insert(newAttachment);

                    if (removedAttachmentIds.Count > 0)
                        await _circularAttachmentService.Delete(removedAttachmentIds);
                }

                return Ok(circularId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating circular : {ex.Message}");
                throw;
            }
        }

        [HttpDelete("{Id}")]
        [ProducesResponseType(typeof(CircularMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var circular = await _circularService.Delete(Id);

                return Ok(circular);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting circular data : {ex.Message}");
                throw;
            }
        }
    }
}
