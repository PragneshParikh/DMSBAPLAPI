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
    public class NewsBulletinController : ControllerBase
    {
        private readonly INewsBulletinService _newsBulletinService;
        private readonly INewsBulletinAttachmentService _newsBulletinAttachmentService;
        private readonly ILogger<NewsBulletinController> _logger;
        public NewsBulletinController(INewsBulletinService newsBulletinService, INewsBulletinAttachmentService newsBulletinAttachmentService, ILogger<NewsBulletinController> logger)
        {
            _newsBulletinService = newsBulletinService;
            _newsBulletinAttachmentService = newsBulletinAttachmentService;
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

                var news = await _newsBulletinService.Get();

                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching the news bulletin records : {ex.Message}");
                throw;
            }
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(NewsBulletinMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetById(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var newsBulletin = await _newsBulletinService.GetById(Id);

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
        [ProducesResponseType(typeof(NewsBulletinMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create(NewsBulletinMasterViewModel newsBulletinMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var newsBulletinId = await _newsBulletinService.Create(newsBulletinMasterViewModel);

                if (newsBulletinId > 0)
                {
                    var newsBulletinAttachmentList = new List<NewsBulletinMasterAttachment>();

                    foreach (var item in newsBulletinMasterViewModel.Files)
                    {
                        var newsBulletinAtt = new NewsBulletinMasterAttachment
                        {
                            NewsBulletinId = newsBulletinId,
                            FileData = item.FileData,
                            FileName = item.FileName,
                            ContentType = item.ContentType,
                            CreatedBy = newsBulletinMasterViewModel.CreatedBy,
                            CreatedDate = newsBulletinMasterViewModel.CreatedDate,

                        };

                        newsBulletinAttachmentList.Add(newsBulletinAtt);
                    }

                    bool result = await _newsBulletinAttachmentService.Insert(newsBulletinAttachmentList);
                }

                return Ok(newsBulletinId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting news : {ex.Message}");
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(NewsBulletinMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(NewsBulletinMasterViewModel newsBulletinMasterViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var newsBulletinId = await _newsBulletinService.Update(newsBulletinMasterViewModel);

                if (newsBulletinId > 0)
                {
                    var newAttachment = new List<NewsBulletinMasterAttachment>();
                    var removedAttachmentIds = new List<int>();

                    foreach (var item in newsBulletinMasterViewModel.Files)
                    {
                        if (item.Status == "Added")
                        {
                            newAttachment.Add(new NewsBulletinMasterAttachment
                            {
                                NewsBulletinId = newsBulletinId,
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
                        await _newsBulletinAttachmentService.Insert(newAttachment);

                    if (removedAttachmentIds.Count > 0)
                        await _newsBulletinAttachmentService.Delete(removedAttachmentIds);
                }

                return Ok(newsBulletinId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating news : {ex.Message}");
                throw;
            }
        }

        [HttpDelete("{Id}")]
        [ProducesResponseType(typeof(NewsBulletinMasterViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var news = await _newsBulletinService.Delete(Id);

                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting news bulletin data : ${ex.Message}");
                throw;
            }
        }
    }
}
