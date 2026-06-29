using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.HSRPService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HSRPController : ControllerBase
    {
        private readonly IHSRPService _hsrpService;
        private readonly string username = "bgauss-dev-user";
        private readonly string password = "cat2024@bgauss";
        public HSRPController(IHSRPService hSRPService)
        {
            _hsrpService = hSRPService;
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPendingHSRPOrders(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                var data = await _hsrpService.GetPendingHSRPListAsync(dealerCode, fromDate, toDate);

                return Ok(data);

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

        [HttpPost("dispatch")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveDispatch([FromBody] HSRPDispatchRequest request)
        {
            try
            {
                await _hsrpService.ReceiveDispatchAsync(request);

                return Ok(new HSRPDispatchResponse
                {
                    Valid = true,
                    Description = "Dispatch detail saved successfully.",
                    Value = new List<HSRPDispatchResponseValue>
            {
                new HSRPDispatchResponseValue
                {
                    Msg = "Dispatch detail saved successfully.",
                    StatusCode = "200",
                    ResponseStatus = "true"
                }
            }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new HSRPDispatchResponse
                {
                    Valid = false,
                    Description = ex.Message,
                    Value = new List<HSRPDispatchResponseValue>
            {
                new HSRPDispatchResponseValue
                {
                    Msg = ex.Message,
                    StatusCode = "400",
                    ResponseStatus = "false"
                }
            }
                });
            }
        }
        //[HttpPost("create")]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> CreateBulkHSRPOrder([FromBody] List<HSRPOrderCreateViwModel> order)
        //{
        //    try
        //    {
        //        var data = await _hsrpService.CreateBulkHSRPOrder(order);
        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new
        //        {
        //            success = false,
        //            message = ex.Message
        //        });
        //    }
        //}

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBulkHSRPOrder([FromBody] List<HSRPOrderCreateViwModel> orders)
        {
            try
            {
                var result = await _hsrpService.CreateBulkHSRPOrder(orders);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }
        //[HttpPut("update")]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> UpdateBulkHSRPOrder([FromBody] List<HSRPOrderCreateViwModel> order)
        //{
        //    try
        //    {
        //        var data = await _hsrpService.UpdateBulkHSRPOrder(order);
        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new
        //        {
        //            success = false,
        //            message = ex.Message
        //        });
        //    }
        //}

        [HttpPut("updateInward")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBulkHSRPInward([FromBody] List<HSRPInwardUpdate> order)
        {
            try
            {
                var data = await _hsrpService.UpdateInwardStatus(order);
                return Ok(data);
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

        [HttpPost("inward")]
        public async Task<IActionResult> UpdateInwardStatus([FromBody] List<HSRPInwardUpdate> orders)
        {
            var result = await _hsrpService.UpdateInwardStatus(orders);

            return Ok(result);
        }
        [HttpGet("hsrpList")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHSRPListAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                var data = await _hsrpService.GetAllHSRPOrderAsync(dealerCode, fromDate, toDate);

                return Ok(data);

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

        [HttpGet("hsrpInwardList")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHSRPInwardListAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                var data = await _hsrpService.GetAllHSRPInward(dealerCode, fromDate, toDate);

                return Ok(data);

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

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _hsrpService.GetAllHSRPOrderByIdAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
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

        [HttpGet("download")]
        [ProducesResponseType(typeof(IEnumerable<RoleWiseMenuRight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download(bool isSuperAdmin, string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                var file = await _hsrpService.DownloadHSRPExcel(isSuperAdmin, dealerCode, fromDate, toDate);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "HSRPList.xlsx"
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

        private async Task<string> GetHSRPLoginTokenAsync()
        {
            using var httpClient = new HttpClient();

            var url = "https://devbgaussapi.rosmertahsrp.com/api/pos/getApiKey";

            var payload = new
            {
                username = username,
                password = password
            };

            var json = JsonSerializer.Serialize(payload);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(url, content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: {response.StatusCode} - {responseString}";
                }
                var tokenResponse = JsonSerializer.Deserialize<HSRPLoginResponseViewModel>(responseString);

                return tokenResponse.Value.AccessToken;
                //return responseString.AccessToken;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
