using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.EmployeeMasterService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        // FIX: previously returned every employee regardless of who was
        // asking. Now scoped to the caller's own DealerCode (already
        // embedded in the JWT for both Dealer and Employee logins) unless
        // the caller is SuperAdmin. Fails closed — no dealer context on the
        // token means an empty list, never "show everyone" by default.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeMaster>>> Get()
        {
            try
            {
                var users = await _employeeService.Get();

                if (!User.IsInRole("SuperAdmin"))
                {
                    var callerDealerCode = User.FindFirst("DealerCode")?.Value;

                    if (string.IsNullOrWhiteSpace(callerDealerCode))
                    {
                        // No dealer context on this token (e.g. a BgEmployee
                        // login, which spans multiple dealers and isn't
                        // handled by this single-DealerCode filter yet).
                        // Fail closed rather than exposing every dealer's data.
                        return Ok(Enumerable.Empty<EmployeeMaster>());
                    }

                    users = users.Where(u =>
                        string.Equals(u.DealerCode?.Trim(), callerDealerCode.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        // GET EMPLOYEE BY ID
        // FIX: also scoped now, so a dealer can't bypass the list filter by
        // guessing/hitting another dealer's employee id directly.
        [HttpGet("GetById/{Id}")]
        public async Task<ActionResult> GetEmployeeById(int Id)
        {
            try
            {
                var user = await _employeeService.GetEmployeeById(Id);

                if (user == null)
                    return NotFound(new { message = $"Employee with ID {Id} not found." });

                if (!User.IsInRole("SuperAdmin"))
                {
                    var callerDealerCode = User.FindFirst("DealerCode")?.Value;

                    if (string.IsNullOrWhiteSpace(callerDealerCode) ||
                        !string.Equals(user.DealerCode?.Trim(), callerDealerCode.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }
                }

                var mappings = await _employeeService.GetRoleMappings(Id);

                var response = new
                {
                    user.Id,
                    user.EmployeeCode,
                    user.FirstName,
                    user.LastName,
                    user.Gender,
                    user.Mobile,
                    user.EmailId,
                    user.Password,
                    user.Address,
                    user.State,
                    user.City,
                    user.Pincode,
                    user.DateOfJoin,
                    user.Designation,
                    user.Department,
                    user.DealerCode,
                    user.LocationCode,
                    user.ProfileImage,
                    user.Supervisor,
                    user.IsActive,
                    user.Notes,
                    user.CreatedBy,
                    user.CreatedDate,
                    user.UpdatedBy,
                    user.UpdatedDate,

                    selectedDepartments = mappings.Select(m => m.Category).Distinct().ToList(),
                    roles = mappings.Select(m => m.RoleName).Distinct().ToList(),
                };

                return Ok(response);
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
                if (!string.IsNullOrWhiteSpace(employeeMaster.EmailId))
                    employeeMaster.EmailId = employeeMaster.EmailId.Trim().ToLowerInvariant();

                var saved = await _employeeService.CreateNewUser(employeeMaster);

                //if (employeeMaster.CreateLogin && !string.IsNullOrWhiteSpace(employeeMaster.EmailId))
                    await EnsureEmployeeLogin(employeeMaster);

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
                if (!string.IsNullOrWhiteSpace(employeeMaster.EmailId))
                    employeeMaster.EmailId = employeeMaster.EmailId.Trim().ToLowerInvariant();

                var result = await _employeeService.UpdateEmployee(employeeMaster);

                if (result == 0)
                    return NotFound("Employee Not Found");

                //if (employeeMaster.CreateLogin && !string.IsNullOrWhiteSpace(employeeMaster.EmailId))
                    await EnsureEmployeeLogin(employeeMaster);

                return Ok(new { message = "Employee Updated Successfully" });
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
                var employeeList = await _employeeService.GetEmployeesByDesignation(dealerCode, designation);

                return Ok(employeeList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Downloads the employee list as an Excel file. If dealerCode is
        /// supplied, exports only that dealer's employees — otherwise exports
        /// everyone. FIX: non-SuperAdmin callers can no longer pass an
        /// arbitrary dealerCode — their own DealerCode (from the token) is
        /// enforced instead, closing the same gap as Get()/GetEmployeeById().
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string? dealerCode = null)
        {
            try
            {
                if (!User.IsInRole("SuperAdmin"))
                {
                    dealerCode = User.FindFirst("DealerCode")?.Value;

                    if (string.IsNullOrWhiteSpace(dealerCode))
                        return Forbid();
                }

                var file = await _employeeService.DownloadEmployeeExcel(dealerCode);

                var fileName = string.IsNullOrWhiteSpace(dealerCode)
                    ? "EmployeeList_All.xlsx"
                    : $"EmployeeList_{dealerCode}.xlsx";

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
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
                var token = await _userManager.GeneratePasswordResetTokenAsync(existing);
                var res = await _userManager.ResetPasswordAsync(existing, token, emp.Password);

                if (!res.Succeeded)
                    throw new Exception($"Login password could not be updated: {string.Join(", ", res.Errors.Select(e => e.Description))}");
            }
        }
    }
}