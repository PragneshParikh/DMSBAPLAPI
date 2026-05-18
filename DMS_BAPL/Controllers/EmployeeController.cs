using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.EmployeeMasterService;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;
        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EmployeeMaster>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var users = await _employeeService.Get();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching employee list : ${ex.Message}");
                throw;
            }
        }

        [HttpGet("GetById/{Id}")]
        [ProducesResponseType(typeof(EmployeeMaster), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeMaster?>> GetEmployeeById(int Id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var user = await _employeeService.GetEmployeeById(Id);

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while featching records by user Id : ${ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(EmployeeMaster), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateNewUser(EmployeeMaster employeeMaster)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var user = await _employeeService.CreateNewUser(employeeMaster);

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while creating a user : ${ex.Message}");
                throw;
            }
        }
    }
}
