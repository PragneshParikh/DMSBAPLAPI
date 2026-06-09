using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.EmployeeMasterService;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeService employeeService,
            ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        // GET ALL EMPLOYEES
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeMaster>>> Get()
        {
            try
            {
                var users = await _employeeService.Get();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        // GET EMPLOYEE BY ID
        [HttpGet("GetById/{Id}")]
        public async Task<ActionResult<EmployeeMaster?>> GetEmployeeById(int Id)
        {
            try
            {
                var user = await _employeeService.GetEmployeeById(Id);

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        // INSERT EMPLOYEE
        [HttpPost]
        public async Task<ActionResult> CreateNewUser(
            [FromBody] EmployeeMaster employeeMaster)
        {
            try
            {
                var user = await _employeeService.CreateNewUser(employeeMaster);

                return Ok(new
                {
                    message = "Employee Saved Successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateEmployee([FromBody] EmployeeMaster employeeMaster)
        {
            try
            {
                var result = await _employeeService.UpdateEmployee(employeeMaster);

                if (result == 0)
                {
                    return NotFound("Employee Not Found");
                }

                return Ok(new
                {
                    message = "Employee Updated Successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("employeeByDesignation")]
        public async Task<ActionResult<IEnumerable<EmployeeMaster>>> Get(string? dealerCode, string designation)
        {
            try
            {
                var employeeList = await _employeeService.GetEmployeesByDesignation(dealerCode,designation);

                return Ok(employeeList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }
    }
}