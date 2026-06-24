using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.EmployeeMasterService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeController(
            IEmployeeService employeeService, ILogger<EmployeeController> logger, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _employeeService = employeeService;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
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
        public async Task<ActionResult> CreateNewUser([FromBody] EmployeeMaster employeeMaster)
        {
            try
            {
                var saved = await _employeeService.CreateNewUser(employeeMaster);

                if (employeeMaster.CreateLogin && !string.IsNullOrWhiteSpace(employeeMaster.EmailId))
                    await EnsureEmployeeLogin(employeeMaster);   // <-- this is the missing call

                return Ok(new { message = "Employee Saved Successfully", data = saved });
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



        // GET DEALER INFO BY CODE
        //[HttpGet("GetDealerByCode/{dealerCode}")]
        //public async Task<ActionResult> GetDealerByCode(string dealerCode)
        //{
        //    try
        //    {
        //        var dealer = await _employeeService.GetDealerByCode(dealerCode);
        //        if (dealer == null) return NotFound("Dealer Not Found");
        //        return Ok(dealer);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return BadRequest(ex.Message);
        //    }
        //}

        // GET DEALER LOCATIONS
        //[HttpGet("GetLocationsByDealer/{dealerCode}")]
        //public async Task<ActionResult> GetLocationsByDealer(string dealerCode)
        //{
        //    try
        //    {
        //        var locations = await _employeeService.GetLocationsByDealer(dealerCode);
        //        return Ok(locations);
        [HttpGet("employeeByDesignation")]
        public async Task<ActionResult<IEnumerable<EmployeeMaster>>> Get(string? dealerCode, string designation)
        {
            try
            {
                var employeeList = await _employeeService.GetEmployeesByDesignation(dealerCode, designation);

                return Ok(employeeList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        private async Task EnsureEmployeeLogin(EmployeeMaster emp)
        {
            if (!await _roleManager.RoleExistsAsync("Employee"))
                await _roleManager.CreateAsync(new IdentityRole("Employee"));

            var existing = await _userManager.FindByEmailAsync(emp.EmailId!);

            if (existing == null)
            {
                // create new account
                var user = new ApplicationUser
                {
                    UserName = emp.EmailId,
                    Email = emp.EmailId,
                    EmailConfirmed = true
                };
                var pwd = string.IsNullOrWhiteSpace(emp.Password) ? "Temp@123" : emp.Password;
                var res = await _userManager.CreateAsync(user, pwd);
                if (!res.Succeeded)
                    throw new Exception($"Login could not be created: {string.Join(", ", res.Errors.Select(e => e.Description))}");
                await _userManager.AddToRoleAsync(user, "Employee");
            }
            else if (!string.IsNullOrWhiteSpace(emp.Password))
            {
                // account exists — re-sync the password to match EmployeeMaster
                var token = await _userManager.GeneratePasswordResetTokenAsync(existing);
                var res = await _userManager.ResetPasswordAsync(existing, token, emp.Password);
                if (!res.Succeeded)
                    throw new Exception($"Login password could not be updated: {string.Join(", ", res.Errors.Select(e => e.Description))}");
            }
        }


    }
}