using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.MaterialTransferService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/material-transfer")]
    [ApiController]
    public class MaterialTransferController : ControllerBase
    {
        private readonly IMaterialTransferService _materialTransferService;
        private readonly ILogger<MaterialTransferController> _logger;

        public MaterialTransferController(IMaterialTransferService materialTransferService, ILogger<MaterialTransferController> logger)
        {
            _materialTransferService = materialTransferService;
            _logger = logger;
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

        [HttpGet("GetByDealerPaged")]
        [ProducesResponseType(typeof(PagedResponse<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<object>>> GetMaterialTransferDetailByDealer(
            [FromQuery] string dealerCode,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var materials = await _materialTransferService.GetMaterialTransferDetailsByDealer(dealerCode, pageIndex, pageSize);

                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in feathing material details by dealer : {ex.Message}");
                throw;
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

                var material = await _materialTransferService.InsertMaterials(materialTransferViewModels);

                return Ok(material);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while insert items : ${ex.Message}");
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
    }
}
