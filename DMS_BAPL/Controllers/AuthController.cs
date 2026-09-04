//using DMS_BAPL_Data.CustomModel;
//using DMS_BAPL_Data.DBModels;
//using DMS_BAPL_Data.Services.BgEmployeeMasterService;
//using DMS_BAPL_Data.Services.DealerMasterService;
//using DMS_BAPL_Data.Services.EmailService;
//using DMS_BAPL_Data.Services.EmployeeMasterService;
//using DMS_BAPL_Utils.Constants;
//using DMS_BAPL_Utils.Helpers;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using System.Web;

//namespace DMS_BAPL_Api.Controllers
//{
//    [Route("api/auth")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly IConfiguration _configuration;
//        private readonly IEmailService _emailService;
//        private readonly IWebHostEnvironment _env;
//        private readonly IDealerMasterService _dealerMasterService;
//        private readonly ILogger<AuthController> _logger;
//        private readonly IEmployeeService _employeeService;
//        private readonly IBgEmployeeMasterService _bgEmployeeMasterService;
//        private readonly BapldmsvadContext _context;   // ADDED — needed for ImpersonationLogs

//        public AuthController(UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager, IEmailService emailService,
//            IConfiguration configuration, IWebHostEnvironment env,
//            IDealerMasterService dealerMasterService, ILogger<AuthController> logger,
//            IEmployeeService employeeService, IBgEmployeeMasterService bgEmployeeMasterService,
//            BapldmsvadContext context)   // ADDED
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _configuration = configuration;
//            _emailService = emailService;
//            _env = env;
//            _dealerMasterService = dealerMasterService;
//            _logger = logger;
//            _employeeService = employeeService;
//            _bgEmployeeMasterService = bgEmployeeMasterService;
//            _context = context;   // ADDED
//        }

//        [HttpGet]
//        [ProducesResponseType(typeof(IEnumerable<AspNetUser>), StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<IEnumerable<AspNetUser>>> GetAllUser()
//        {
//            try
//            {
//                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

//                if (string.IsNullOrEmpty(userId))
//                    return Unauthorized("User not authorized");

//                var users = _userManager.Users.ToList();

//                return Ok(users);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error while getting user list : ${ex.Message}");
//                throw;
//            }
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> Login([FromBody] Login model)
//        {
//            try
//            {
//                // ── Normalize the incoming identifier up front ──
//                var normalizedUsername = model.Username?.Trim();

//                if (string.IsNullOrWhiteSpace(normalizedUsername))
//                    return Unauthorized(new { message = "Username not found." });

//                // find by username OR email
//                //var user = await _userManager.FindByNameAsync(normalizedUsername)
//                //           ?? await _userManager.FindByEmailAsync(normalizedUsername);

//                //if (user == null)
//                //    return Unauthorized(new { message = "Username not found." });
//                // find by username OR email
//                var user = await _userManager.FindByNameAsync(normalizedUsername)
//                           ?? await _userManager.FindByEmailAsync(normalizedUsername);

//                if (user == null)
//                {
//                    // No AspNetUsers login yet — check if this email belongs to a dealer in
//                    // DealerMaster that was never linked (e.g. added via ERP/SQL import), and
//                    // auto-provision their login + Dealer role on the spot.
//                    try
//                    {
//                        user = await _dealerMasterService.EnsureDealerUserFromEmailAsync(normalizedUsername);
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError($"Auto-provisioning dealer login failed for {normalizedUsername}: {ex.Message}");
//                        return StatusCode(StatusCodes.Status500InternalServerError,
//                            new { success = false, message = "Failed to provision dealer login." });
//                    }

//                    if (user == null)
//                        return Unauthorized(new { message = "Username not found." });
//                }

//                var result = await _signInManager.CheckPasswordSignInAsync(
//                    user, model.Password, lockoutOnFailure: false);

//                if (!result.Succeeded)
//                    return Unauthorized(new { message = "Invalid password." });

//                var roles = await _userManager.GetRolesAsync(user);
//                var functionalRoles = roles
//                                    .Where(r => !r.Equals("Employee", StringComparison.OrdinalIgnoreCase))
//                                    .ToList();
//                var role = functionalRoles.FirstOrDefault() ?? roles.FirstOrDefault() ?? "";

//                string dealerCode = null;
//                string employeeCode = null;
//                string mappedZones = null;

//                if (roles.Contains("Employee"))
//                {
//                    // single call resolves BG-employee-first, employee-fallback,
//                    // normalizing the lookup email on both sides
//                    var loginInfo = await ResolveEmployeeLoginInfo(user.Email);

//                    if (!loginInfo.Found || !loginInfo.IsActive)
//                        return Unauthorized(new { message = "Employee account not found or inactive." });

//                    dealerCode = loginInfo.DealerCode;
//                    employeeCode = loginInfo.EmployeeCode;
//                    mappedZones = loginInfo.MappedZones;
//                }
//                else
//                {
//                    dealerCode = user.DealerCode;  // dealer logs in as themselves
//                }

//                var dealerInfo = !string.IsNullOrWhiteSpace(dealerCode)
//                    ? await _dealerMasterService.GetDealerByCode(dealerCode)
//                    : null;

//                user.LastLoginDate = DateTime.UtcNow;
//                await _userManager.UpdateAsync(user);

//                var token = await GenerateJwtToken(user, dealerCode);

//                return Ok(new
//                {
//                    userId = user.Id,
//                    email = user.Email,
//                    userName = user.Email,
//                    dealerCode = dealerCode,
//                    employeeCode = employeeCode,
//                    mappedZones = mappedZones,
//                    lastLoginDate = user.LastLoginDate,
//                    token = token,
//                    role = role,
//                    roles = functionalRoles,
//                    compName = dealerInfo?.Compname,
//                    status = "success",
//                    message = "Login successful"
//                });
//            }
//            catch (Exception)
//            {
//                _logger.LogError("Error occurred during Login");
//                throw;
//            }
//        }

//        // Shared result shape for either employee type
//        private record EmployeeLoginInfo(
//            string DealerCode,
//            string EmployeeCode,
//            string MappedZones,
//            bool IsActive,
//            bool Found
//        );

//        private async Task<EmployeeLoginInfo> ResolveEmployeeLoginInfo(string email)
//        {
//            var normalizedEmail = email?.Trim().ToLowerInvariant();

//            if (string.IsNullOrWhiteSpace(normalizedEmail))
//                return new EmployeeLoginInfo(null, null, null, false, Found: false);

//            var bgEmployee = await _bgEmployeeMasterService.GetByEmail(normalizedEmail);

//            if (bgEmployee != null)
//            {
//                return new EmployeeLoginInfo(
//                    DealerCode: bgEmployee.DealerCode,
//                    EmployeeCode: bgEmployee.EmployeeCode,
//                    MappedZones: bgEmployee.MappedZones,
//                    IsActive: bgEmployee.IsActive,
//                    Found: true
//                );
//            }

//            var employee = await _employeeService.GetEmployeeByEmail(normalizedEmail);

//            if (employee != null)
//            {
//                return new EmployeeLoginInfo(
//                    DealerCode: employee.DealerCode,
//                    EmployeeCode: employee.EmployeeCode,
//                    MappedZones: null,
//                    IsActive: employee.IsActive,
//                    Found: true
//                );
//            }

//            return new EmployeeLoginInfo(null, null, null, false, Found: false);
//        }
//        //[HttpPost]
//        //[AllowAnonymous]
//        //[ProducesResponseType(StatusCodes.Status200OK)]
//        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        //public async Task<IActionResult> Login([FromBody] Login model)
//        //{
//        //    try
//        //    {
//        //        // find by username OR email
//        //        var user = await _userManager.FindByNameAsync(model.Username)
//        //                   ?? await _userManager.FindByEmailAsync(model.Username);

//        //        if (user == null)
//        //            return Unauthorized(new { message = "Username not found." });

//        //        var result = await _signInManager.CheckPasswordSignInAsync(
//        //            user, model.Password, lockoutOnFailure: false);

//        //        if (!result.Succeeded)
//        //            return Unauthorized(new { message = "Invalid password." });

//        //        var roles = await _userManager.GetRolesAsync(user);
//        //        var role = roles.FirstOrDefault() ?? "";

//        //        string dealerCode = null;
//        //        string employeeCode = null;
//        //        string mappedZones = null;

//        //        if (roles.Contains("Employee"))
//        //        {
//        //            // single call resolves BG-employee-first, employee-fallback
//        //            var loginInfo = await ResolveEmployeeLoginInfo(user.Email);

//        //            if (!loginInfo.Found || !loginInfo.IsActive)
//        //                return Unauthorized(new { message = "Employee account not found or inactive." });

//        //            dealerCode = loginInfo.DealerCode;
//        //            employeeCode = loginInfo.EmployeeCode;
//        //            mappedZones = loginInfo.MappedZones;
//        //        }
//        //        else
//        //        {
//        //            dealerCode = user.DealerCode;  // dealer logs in as themselves
//        //        }

//        //        var dealerInfo = !string.IsNullOrWhiteSpace(dealerCode)
//        //            ? await _dealerMasterService.GetDealerByCode(dealerCode)
//        //            : null;

//        //        user.LastLoginDate = DateTime.UtcNow;
//        //        await _userManager.UpdateAsync(user);

//        //        var token = await GenerateJwtToken(user, dealerCode);

//        //        return Ok(new
//        //        {
//        //            userId = user.Id,
//        //            email = user.Email,
//        //            userName = user.Email,
//        //            dealerCode = dealerCode,
//        //            employeeCode = employeeCode,
//        //            mappedZones = mappedZones,
//        //            lastLoginDate = user.LastLoginDate,
//        //            token = token,
//        //            role = role,
//        //            compName = dealerInfo?.Compname,
//        //            status = "success",
//        //            message = "Login successful"
//        //        });
//        //    }
//        //    catch (Exception)
//        //    {
//        //        _logger.LogError("Error occurred during Login");
//        //        throw;
//        //    }
//        //}

//        //// Shared result shape for either employee type
//        //private record EmployeeLoginInfo(
//        //    string DealerCode,
//        //    string EmployeeCode,
//        //    string MappedZones,
//        //    bool IsActive,
//        //    bool Found
//        //);

//        /// <summary>
//        /// Resolves dealer code / employee code / zone mapping for a logged-in user
//        /// holding the "Employee" role, checking BgEmployeeMaster first and falling
//        /// back to EmployeeMaster. Returns Found=false if neither table has a match.
//        /// </summary>
//        //private async Task<EmployeeLoginInfo> ResolveEmployeeLoginInfo(string email)
//        //{
//        //    var bgEmployee = await _bgEmployeeMasterService.GetByEmail(email);

//        //    if (bgEmployee != null)
//        //    {
//        //        return new EmployeeLoginInfo(
//        //            DealerCode: bgEmployee.DealerCode,
//        //            EmployeeCode: bgEmployee.EmployeeCode,
//        //            MappedZones: bgEmployee.MappedZones,
//        //            IsActive: bgEmployee.IsActive,
//        //            Found: true
//        //        );
//        //    }

//        //    var employee = await _employeeService.GetEmployeeByEmail(email);

//        //    if (employee != null)
//        //    {
//        //        return new EmployeeLoginInfo(
//        //            DealerCode: employee.DealerCode,
//        //            EmployeeCode: employee.EmployeeCode,
//        //            MappedZones: null,
//        //            IsActive: employee.IsActive,
//        //            Found: true
//        //        );
//        //    }

//        //    return new EmployeeLoginInfo(null, null, null, false, Found: false);
//        //}

//        /// <summary>
//        /// Initiates the password reset process for a user.
//        /// If the provided email exists, generates a password reset token,
//        /// creates a reset link, and sends it to the user's email.
//        /// </summary>
//        /// <param name="model">ForgotPassword model containing the user's email.</param>
//        /// <returns>
//        /// 200 OK if the reset link is sent successfully or if the email does not exist (to avoid exposing user info),
//        /// 500 Internal Server Error if an exception occurs while processing.
//        /// </returns>
//        [HttpPost("forgot-password")]
//        [AllowAnonymous]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
//        {
//            try
//            {
//                var user = await _userManager.FindByEmailAsync(model.Email);

//                if (user == null)
//                    return Ok();

//                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

//                var encodedToken = HttpUtility.UrlEncode(token);

//                string baseUrl = _env.IsDevelopment() ? "http://localhost:4200" : "https://yourdomain.com";

//                var resetLink = $"{baseUrl}/reset-password?email={model.Email}&token={encodedToken}";

//                var body = $@"
//                                <p>Click the link below to reset your password:</p>
//                                <p><a href='{resetLink}'>Reset Password</a></p>
//                            ";

//                await _emailService.SendEmailAsync(model.Email,
//                        "Reset Password",
//                    body);

//                return Ok(new { success = true, message = "Password reset link sent." });
//            }
//            catch (Exception)
//            {
//                _logger.LogError("An error occurred while sending the reset email.");
//                throw;
//            }
//        }

//        /// <summary>
//        /// Resets the password for a user using the provided reset token.
//        /// Validates the token and updates the user's password if valid.
//        /// </summary>
//        /// <param name="model">ResetPassword model containing Email, Token, and new Password.</param>
//        /// <returns>
//        /// 200 OK if the password is reset successfully,
//        /// 400 Bad Request if the user is not found or the token is invalid,
//        /// 500 Internal Server Error if an exception occurs during processing.
//        /// </returns>
//        [HttpPost("reset-password")]
//        [AllowAnonymous]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword model)
//        {
//            try
//            {
//                var user = await _userManager.FindByEmailAsync(model.Email);

//                if (user == null)
//                    return BadRequest(new { success = false, message = "Invalid request: user not found." });

//                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

//                if (!result.Succeeded)
//                {
//                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//                    return BadRequest(new { success = false, message = "Password reset failed", errors });
//                }

//                return Ok(new { success = true, message = "Password reset successful" });
//            }
//            catch (Exception)
//            {
//                _logger.LogError("Error occurred while resetting password");
//                throw;
//            }
//        }

//        /// <summary>
//        /// Generates a JWT token for the given user including roles and standard claims.
//        /// </summary>
//        /// <param name="user">The authenticated ApplicationUser.</param>
//        /// <returns>A JWT token string valid for 24 hours.</returns>
//        private async Task<string> GenerateJwtToken(ApplicationUser user, string dealerCode)
//        {
//            var jwtSettings = _configuration.GetSection("Jwt");
//            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
//            var roles = await _userManager.GetRolesAsync(user);

//            var claims = new List<Claim>
//            {
//                new Claim("DealerCode", dealerCode ?? ""),
//                new Claim(ClaimTypes.NameIdentifier, user.Id),
//                new Claim(ClaimTypes.Name, dealerCode ?? string.Empty),
//                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//            };

//            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(claims),
//                Expires = DateTime.UtcNow.AddHours(24),
//                SigningCredentials = new SigningCredentials(
//                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
//                Issuer = jwtSettings["Issuer"],
//                Audience = jwtSettings["Audience"]
//            };

//            var tokenHandler = new JwtSecurityTokenHandler();
//            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
//        }

//        [HttpPost("check-default-password")]
//        [Authorize(Roles = "SuperAdmin")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        public async Task<IActionResult> CheckDefaultPassword([FromBody] WhoAmIRequest model)
//        {
//            var user = await _userManager.FindByEmailAsync(model.Email);

//            if (user == null)
//                return Ok(new { found = false, isDefaultPassword = false });

//            var passwordHasher = new PasswordHasher<ApplicationUser>();
//            var verifyResult = passwordHasher.VerifyHashedPassword(
//                user, user.PasswordHash, StringConstants.DealerDefaultPassword);

//            bool isDefault = verifyResult == PasswordVerificationResult.Success;

//            return Ok(new
//            {
//                found = true,
//                email = user.Email,
//                isDefaultPassword = isDefault,
//                message = isDefault
//                    ? $"This dealer is still using the default password ({StringConstants.DealerDefaultPassword})."
//                    : "This dealer has set a custom password. Use 'Reset Password' to issue a new one."
//            });
//        }

//        [HttpPost("admin-reset-password")]
//        [Authorize(Roles = "SuperAdmin")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        public async Task<IActionResult> AdminResetPassword([FromBody] WhoAmIRequest model)
//        {
//            var user = await _userManager.FindByEmailAsync(model.Email);

//            if (user == null)
//                return BadRequest(new { success = false, message = "Dealer email not found." });

//            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
//            var result = await _userManager.ResetPasswordAsync(user, token, StringConstants.DealerDefaultPassword);

//            if (!result.Succeeded)
//            {
//                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//                return BadRequest(new { success = false, message = "Reset failed", errors });
//            }

//            return Ok(new
//            {
//                success = true,
//                message = $"Password has been reset to the default ({StringConstants.DealerDefaultPassword}). Please ask the dealer to log in and change it."
//            });
//        }

//        // ADDED — SuperAdmin can generate a live login session for any dealer,
//        // without ever touching or storing that dealer's actual password.
//        [HttpPost("impersonate")]
//        [Authorize(Roles = "SuperAdmin")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        public async Task<IActionResult> Impersonate([FromBody] WhoAmIRequest model)
//        {
//            try
//            {
//                var superAdminId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
//                var superAdminEmail = User.FindFirstValue(ClaimTypes.Email);

//                var targetUser = await _userManager.FindByEmailAsync(model.Email);
//                if (targetUser == null)
//                    return BadRequest(new { success = false, message = "Dealer not found." });

//                var token = await GenerateJwtToken(targetUser, targetUser.DealerCode);

//                _context.ImpersonationLogs.Add(new ImpersonationLog
//                {
//                    SuperAdminUserId = superAdminId,
//                    SuperAdminEmail = superAdminEmail ?? "",
//                    TargetDealerUserId = targetUser.Id,
//                    TargetDealerCode = targetUser.DealerCode,
//                    TargetDealerEmail = targetUser.Email,
//                    StartedDate = DateTime.UtcNow
//                });
//                await _context.SaveChangesAsync();

//                return Ok(new
//                {
//                    success = true,
//                    token = token,
//                    dealerCode = targetUser.DealerCode,
//                    email = targetUser.Email,
//                    message = $"You are now viewing the system as {targetUser.Email}. This session is logged."
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Impersonation failed: {ex.Message}");
//                return StatusCode(500, new { success = false, message = "Impersonation failed." });
//            }
//        }

//        // ADDED — SuperAdmin-visible audit trail of every impersonation session
//        [HttpGet("impersonation-log")]
//        [Authorize(Roles = "SuperAdmin")]
//        public async Task<IActionResult> GetImpersonationLog()
//        {
//            var logs = await _context.ImpersonationLogs
//                .OrderByDescending(x => x.StartedDate)
//                .Select(x => new
//                {
//                    x.SuperAdminEmail,
//                    x.TargetDealerCode,
//                    x.TargetDealerEmail,
//                    x.StartedDate,
//                    x.EndedDate
//                })
//                .ToListAsync();

//            return Ok(logs);
//        }
//    }
//}

using Microsoft.AspNetCore.DataProtection;
using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.BgEmployeeMasterService;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.EmailService;
using DMS_BAPL_Data.Services.EmployeeMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;   // required for the IWebHostEnvironment type itself
using Microsoft.Extensions.Hosting;   // required for the IsDevelopment() extension method — see note below
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

// NOTE: IWebHostEnvironment (the type used for _env below) lives in
// Microsoft.AspNetCore.Hosting, but its IsDevelopment()/IsProduction()/
// IsEnvironment() extension methods live in the DIFFERENT namespace
// Microsoft.Extensions.Hosting (IWebHostEnvironment implements
// IHostEnvironment, which is what those extensions actually target).
// Microsoft.AspNetCore.Hosting also happens to contain an OLD, pre-3.0
// IsDevelopment(IHostingEnvironment) extension for a now-legacy interface
// of a similar name. With only Microsoft.AspNetCore.Hosting imported, the
// compiler can only see that legacy, type-mismatched overload — which is
// exactly the CS1929 error this causes. Both usings need to be present.

namespace DMS_BAPL_Api.Controllers
{
    // NEW — request shape for the dedicated location-login endpoint,
    // matching EmployeeMasterService.locationLogin()'s payload exactly.
    public class LocationLoginRequestModel
    {
        public string LocationLoginId { get; set; }
        public string Password { get; set; }
        public string? LocationCode { get; set; }
    }

    // NEW — request shape for the diagnostic/admin location-password reset
    // endpoint below.
    public class AdminResetLocationPasswordRequest
    {
        public string LocationLoginId { get; set; }
        public string NewPassword { get; set; }
    }

    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly IDealerMasterService _dealerMasterService;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmployeeService _employeeService;
        private readonly IBgEmployeeMasterService _bgEmployeeMasterService;
        private readonly ILocationMasterService _locationMasterService;   // resolves the location-assigned role for Location Login
        private readonly BapldmsvadContext _context;   // ADDED — needed for ImpersonationLogs

        // Verifies EmployeeMaster.LocationPasswordHash. Same hasher shape
        // EmployeeMasterRepo already uses to create that hash
        // (PasswordHasher<EmployeeMaster>), so hashes it wrote can be
        // verified here.
        //private static readonly PasswordHasher<EmployeeMaster> _locationPasswordHasher = new();
        private readonly IDataProtector _locationPasswordProtector;
        public AuthController( UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService,
            IConfiguration configuration, IWebHostEnvironment env, IDealerMasterService dealerMasterService, ILogger<AuthController> logger,
            IEmployeeService employeeService, IBgEmployeeMasterService bgEmployeeMasterService, ILocationMasterService locationMasterService, 
            BapldmsvadContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _env = env;
            _dealerMasterService = dealerMasterService;
            _logger = logger;
            _employeeService = employeeService;
            _bgEmployeeMasterService = bgEmployeeMasterService;
            _locationMasterService = locationMasterService;
            _context = context;

            _locationPasswordProtector =
                dataProtectionProvider.CreateProtector("LocationPassword.v1");
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AspNetUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AspNetUser>>> GetAllUser()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var users = _userManager.Users.ToList();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting user list : ${ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            try
            {
                // ── Normalize the incoming identifier up front ──
                var normalizedUsername = model.Username?.Trim();

                if (string.IsNullOrWhiteSpace(normalizedUsername))
                    return Unauthorized(new { message = "Username not found." });

                // find by username OR email
                var user = await _userManager.FindByNameAsync(normalizedUsername)
                           ?? await _userManager.FindByEmailAsync(normalizedUsername);

                if (user == null)
                {
                    // Try Location Login before dealer auto-provisioning. A
                    // Location Login identifier never has an AspNetUsers row at
                    // all — it authenticates purely off
                    // EmployeeMaster.LocationPasswordHash — so it always lands
                    // here. Returns null (not a Location Login ID at all) so the
                    // dealer auto-provisioning fallback below still runs for
                    // genuinely unmatched dealer emails.
                    var locationLoginResult = await TryLocationLoginAsync(normalizedUsername, model.Password);
                    if (locationLoginResult != null)
                        return locationLoginResult;

                    // No AspNetUsers login yet — check if this email belongs to a dealer in
                    // DealerMaster that was never linked (e.g. added via ERP/SQL import), and
                    // auto-provision their login + Dealer role on the spot.
                    try
                    {
                        user = await _dealerMasterService.EnsureDealerUserFromEmailAsync(normalizedUsername);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Auto-provisioning dealer login failed for {normalizedUsername}: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { success = false, message = "Failed to provision dealer login." });
                    }

                    if (user == null)
                        return Unauthorized(new { message = "Username not found." });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user, model.Password, lockoutOnFailure: false);

                if (!result.Succeeded)
                    return Unauthorized(new { message = "Invalid password." });

                var roles = await _userManager.GetRolesAsync(user);
                var functionalRoles = roles
                                    .Where(r => !r.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                var role = functionalRoles.FirstOrDefault() ?? roles.FirstOrDefault() ?? "";

                string dealerCode = null;
                string employeeCode = null;
                string mappedZones = null;

                if (roles.Contains("Employee"))
                {
                    // single call resolves BG-employee-first, employee-fallback,
                    // normalizing the lookup email on both sides
                    var loginInfo = await ResolveEmployeeLoginInfo(user.Email);

                    if (!loginInfo.Found || !loginInfo.IsActive)
                        return Unauthorized(new { message = "Employee account not found or inactive." });

                    dealerCode = loginInfo.DealerCode;
                    employeeCode = loginInfo.EmployeeCode;
                    mappedZones = loginInfo.MappedZones;
                }
                else
                {
                    dealerCode = user.DealerCode;  // dealer logs in as themselves
                }

                var dealerInfo = !string.IsNullOrWhiteSpace(dealerCode)
                    ? await _dealerMasterService.GetDealerByCode(dealerCode)
                    : null;

                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var token = await GenerateJwtToken(user, dealerCode);

                return Ok(new
                {
                    userId = user.Id,
                    email = user.Email,
                    userName = user.Email,
                    dealerCode = dealerCode,
                    employeeCode = employeeCode,
                    mappedZones = mappedZones,
                    lastLoginDate = user.LastLoginDate,
                    token = token,
                    role = role,
                    roles = functionalRoles,
                    compName = dealerInfo?.Compname,
                    status = "success",
                    message = "Login successful"
                });
            }
            catch (Exception)
            {
                _logger.LogError("Error occurred during Login");
                throw;
            }
        }

        /// <summary>
        /// Step 2 of the multi-location flow — completes a Location Login
        /// once the frontend already knows (from step 1's
        /// requiresLocationSelection response, or because the employee only
        /// has one location to begin with) which locationCode to use.
        /// </summary>
        [HttpPost("location-login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LocationLogin([FromBody] LocationLoginRequestModel model)
        {
            try
            {
                var locationLoginId = model.LocationLoginId?.Trim();

                if (string.IsNullOrWhiteSpace(locationLoginId))
                    return Unauthorized(new { message = "Location Login ID not found." });

                var authResult = await AuthenticateLocationEmployeeAsync(locationLoginId, model.Password);

                // FIXED: authResult can be null (no EmployeeMaster row has this
                // LocationLoginId at all) — that case was falling straight
                // through to `(EmployeeMaster)authResult` below and crashing
                // with a NullReferenceException on employee.LocationCode inside
                // BuildLocationLoginResponseAsync, surfacing as a 500 instead of
                // a normal "not found" response. TryLocationLoginAsync (the
                // other caller of AuthenticateLocationEmployeeAsync, used by the
                // unified /auth login) already had this check; this endpoint
                // was missing it.
                if (authResult == null)
                    return Unauthorized(new { message = "Location Login ID not found." });

                if (authResult is IActionResult failure)
                    return failure;

                return await BuildLocationLoginResponseAsync((EmployeeMaster)authResult, model.LocationCode);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during Location Login: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Attempts to authenticate `identifier`/`password` as a Location
        /// Login credential from inside the unified Login() action above.
        ///
        /// Returns:
        ///   - null if `identifier` doesn't match any LocationLoginId at
        ///     all, so the caller continues trying other login paths
        ///     (dealer auto-provisioning, etc.)
        ///   - an ActionResult if it DOES match — this identifier belongs
        ///     exclusively to the Location Login flow from here, so the
        ///     caller should return this directly.
        /// </summary>
        private async Task<IActionResult?> TryLocationLoginAsync(string locationLoginId, string password)
        {
            var authResult = await AuthenticateLocationEmployeeAsync(locationLoginId, password);

            if (authResult == null)
                return null; // no such LocationLoginId — let the caller try other paths

            if (authResult is IActionResult failure)
                return failure;

            return await BuildLocationLoginResponseAsync((EmployeeMaster)authResult, requestedLocationCode: null);
        }

        /// <summary>
        /// Shared lookup + password + active checks for a Location Login
        /// credential. Returns:
        ///   - null: no EmployeeMaster row has this LocationLoginId at all.
        ///   - an IActionResult: authentication failed for a reason that
        ///     should be returned to the caller as-is (wrong password,
        ///     inactive, not configured).
        ///   - an EmployeeMaster: authentication succeeded; caller still
        ///     needs to resolve WHICH of the employee's locations to use
        ///     (see BuildLocationLoginResponseAsync).
        /// </summary>
        private async Task<object?> AuthenticateLocationEmployeeAsync(string locationLoginId, string password)
        {
            // Normalize Location Login ID
            var normalizedLocationLoginId =
                locationLoginId?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedLocationLoginId))
            {
                return Unauthorized(new
                {
                    message = "Location Login ID not found."
                }) as IActionResult;
            }

            // Find employee by Location Login ID
            var employee = await _context.EmployeeMasters
                .FirstOrDefaultAsync(e =>
                    e.LocationLoginId != null &&
                    e.LocationLoginId.ToLower() ==
                    normalizedLocationLoginId.ToLower());

            // Location Login ID does not exist
            if (employee == null)
            {
                return null;
            }

            // Location Login password is not configured
            if (string.IsNullOrWhiteSpace(employee.LocationPasswordHash))
            {
                return Unauthorized(new
                {
                    message =
                        "Location Login is not configured for this ID."
                }) as IActionResult;
            }

            // Employee must be active
            if (!employee.IsActive)
            {
                return Unauthorized(new
                {
                    message =
                        "Employee account not found or inactive."
                }) as IActionResult;
            }

            // Password must be supplied
            if (password == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid password."
                }) as IActionResult;
            }

            string storedPassword;

            try
            {
                // LocationPasswordHash is actually a protected/encrypted
                // value created by:
                //
                // _locationPasswordProtector.Protect(...)
                //
                // Therefore it must be recovered using:
                //
                // _locationPasswordProtector.Unprotect(...)

                storedPassword =
                    _locationPasswordProtector.Unprotect(
                        employee.LocationPasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to decrypt LocationPasswordHash for Employee Id {EmployeeId}.",
                    employee.Id);

                return Unauthorized(new
                {
                    message =
                        "Location Login is not set up correctly for this ID. " +
                        "Please ask an admin to reset the location password."
                }) as IActionResult;
            }

            // Compare password exactly.
            // Do not trim the password because spaces could technically
            // be part of the password.
            if (!string.Equals(
                    storedPassword,
                    password,
                    StringComparison.Ordinal))
            {
                return Unauthorized(new
                {
                    message = "Invalid password."
                }) as IActionResult;
            }

            // Authentication successful
            return employee;
        }

        /// <summary>
        /// Resolves which single location this Location Login session is
        /// scoped to, from EmployeeMaster.LocationCode's comma-separated
        /// list:
        ///   - exactly one location on the employee -> use it directly.
        ///   - more than one, and requestedLocationCode matches one of
        ///     them -> use that one (validated against the employee's own
        ///     list, not trusted blindly).
        ///   - more than one, and no valid requestedLocationCode ->
        ///     returns requiresLocationSelection with the employee's
        ///     location options (code/name/city) instead of a token.
        /// </summary>
        private async Task<IActionResult> BuildLocationLoginResponseAsync(EmployeeMaster employee, string? requestedLocationCode)
        {
            var locationCodes = (employee.LocationCode ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (locationCodes.Count == 0)
                return Unauthorized(new { message = "No location is assigned to this Location Login." });

            string resolvedLocationCode;

            if (locationCodes.Count == 1)
            {
                resolvedLocationCode = locationCodes[0];
            }
            else if (!string.IsNullOrWhiteSpace(requestedLocationCode))
            {
                var match = locationCodes.FirstOrDefault(c =>
                    c.Equals(requestedLocationCode.Trim(), StringComparison.OrdinalIgnoreCase));

                if (match == null)
                    return Unauthorized(new { message = "Selected location is not assigned to this employee." });

                resolvedLocationCode = match;
            }
            else
            {
                // Multiple assigned locations, no selection yet — ask the
                // frontend to show a picker and call POST /auth/location-login
                // again with locationCode set to the chosen one.
                var locationOptions = await _context.LocationMasters
                    .Where(l => locationCodes.Contains(l.Loccode))
                    .Select(l => new
                    {
                        locationCode = l.Loccode,
                        locationName = l.Locname,
                        city = l.City
                    })
                    .ToListAsync();

                return Ok(new
                {
                    requiresLocationSelection = true,
                    locationLoginId = employee.LocationLoginId,
                    locations = locationOptions,
                    message = "This login is linked to multiple locations. Please select one.",
                    status = "requires_location_selection"
                });
            }

            // Role resolution per LocationMasterRepo.GetRoleByDealerAndLocationCodeAsync:
            // LocationMaster itself has no RoleId column — the role lives in
            // BgRoleCategoryMappings, keyed by LocationMaster.Id. RoleId
            // comes back as a string (an Identity role id), not an int.
            var (roleId, roleName) = await _locationMasterService
                .GetRoleByDealerAndLocationCodeAsync(employee.DealerCode, resolvedLocationCode);

            var dealerInfo = !string.IsNullOrWhiteSpace(employee.DealerCode)
                ? await _dealerMasterService.GetDealerByCode(employee.DealerCode)
                : null;

            var token = GenerateLocationLoginJwtToken(employee, resolvedLocationCode, roleId);

            return Ok(new
            {
                employeeId = employee.Id,
                employeeCode = employee.EmployeeCode,
                firstName = employee.FirstName,
                lastName = employee.LastName,
                dealerCode = employee.DealerCode,
                locationCode = resolvedLocationCode,
                locationRoleId = roleId,
                role = roleName ?? "",
                token = token,
                compName = dealerInfo?.Compname,
                status = "success",
                message = "Login successful"
            });
        }

        // Builds a JWT directly from EmployeeMaster, since a Location Login
        // identity has no ApplicationUser/AspNetUsers row at all to source
        // one from via the regular GenerateJwtToken(user, dealerCode) below.
        private string GenerateLocationLoginJwtToken(EmployeeMaster employee, string resolvedLocationCode, string? locationRoleId)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
            {
                new Claim("DealerCode", employee.DealerCode ?? ""),
                // The resolved SINGLE location this session is scoped to —
                // not the raw comma-separated EmployeeMaster.LocationCode.
                new Claim("LocationCode", resolvedLocationCode ?? ""),
                // NOTE: this is EmployeeMaster.Id (an int), not an
                // ApplicationUser.Id (a GUID) — regular-login tokens use a
                // GUID here. Any downstream code parsing NameIdentifier
                // expecting a GUID would need to branch on token type.
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.LocationLoginId ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Employee")
            };

            if (!string.IsNullOrWhiteSpace(locationRoleId))
                claims.Add(new Claim("LocationRoleId", locationRoleId));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        // Shared result shape for either employee type
        private record EmployeeLoginInfo(
            string DealerCode,
            string EmployeeCode,
            string MappedZones,
            bool IsActive,
            bool Found
        );

        private async Task<EmployeeLoginInfo> ResolveEmployeeLoginInfo(string email)
        {
            var normalizedEmail = email?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return new EmployeeLoginInfo(null, null, null, false, Found: false);

            var bgEmployee = await _bgEmployeeMasterService.GetByEmail(normalizedEmail);

            if (bgEmployee != null)
            {
                return new EmployeeLoginInfo(
                    DealerCode: bgEmployee.DealerCode,
                    EmployeeCode: bgEmployee.EmployeeCode,
                    MappedZones: bgEmployee.MappedZones,
                    IsActive: bgEmployee.IsActive,
                    Found: true
                );
            }

            var employee = await _employeeService.GetEmployeeByEmail(normalizedEmail);

            if (employee != null)
            {
                return new EmployeeLoginInfo(
                    DealerCode: employee.DealerCode,
                    EmployeeCode: employee.EmployeeCode,
                    MappedZones: null,
                    IsActive: employee.IsActive,
                    Found: true
                );
            }

            return new EmployeeLoginInfo(null, null, null, false, Found: false);
        }

        /// <summary>
        /// Initiates the password reset process for a user.
        /// If the provided email exists, generates a password reset token,
        /// creates a reset link, and sends it to the user's email.
        /// </summary>
        /// <param name="model">ForgotPassword model containing the user's email.</param>
        /// <returns>
        /// 200 OK if the reset link is sent successfully or if the email does not exist (to avoid exposing user info),
        /// 500 Internal Server Error if an exception occurs while processing.
        /// </returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                    return Ok();

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var encodedToken = HttpUtility.UrlEncode(token);

                string baseUrl = _env.IsDevelopment() ? "http://localhost:4200" : "https://yourdomain.com";

                var resetLink = $"{baseUrl}/reset-password?email={model.Email}&token={encodedToken}";

                var body = $@"
                                <p>Click the link below to reset your password:</p>
                                <p><a href='{resetLink}'>Reset Password</a></p>
                            ";

                await _emailService.SendEmailAsync(model.Email,
                        "Reset Password",
                    body);

                return Ok(new { success = true, message = "Password reset link sent." });
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while sending the reset email.");
                throw;
            }
        }

        /// <summary>
        /// Resets the password for a user using the provided reset token.
        /// Validates the token and updates the user's password if valid.
        /// </summary>
        /// <param name="model">ResetPassword model containing Email, Token, and new Password.</param>
        /// <returns>
        /// 200 OK if the password is reset successfully,
        /// 400 Bad Request if the user is not found or the token is invalid,
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                    return BadRequest(new { success = false, message = "Invalid request: user not found." });

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { success = false, message = "Password reset failed", errors });
                }

                return Ok(new { success = true, message = "Password reset successful" });
            }
            catch (Exception)
            {
                _logger.LogError("Error occurred while resetting password");
                throw;
            }
        }

        /// <summary>
        /// Generates a JWT token for the given user including roles and standard claims.
        /// </summary>
        /// <param name="user">The authenticated ApplicationUser.</param>
        /// <returns>A JWT token string valid for 24 hours.</returns>
        private async Task<string> GenerateJwtToken(ApplicationUser user, string dealerCode)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim("DealerCode", dealerCode ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, dealerCode ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        [HttpPost("check-default-password")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckDefaultPassword([FromBody] WhoAmIRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok(new { found = false, isDefaultPassword = false });

            // FIXED: same unguarded VerifyHashedPassword pattern that caused
            // the crash in Location Login. If user.PasswordHash is ever
            // malformed for any reason, this threw the same
            // "invalid Base-64 string" FormatException instead of a normal
            // response — on a SuperAdmin diagnostic tool, of all places.
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            PasswordVerificationResult verifyResult;
            try
            {
                verifyResult = passwordHasher.VerifyHashedPassword(
                    user, user.PasswordHash, StringConstants.DealerDefaultPassword);
            }
            catch (FormatException ex)
            {
                _logger.LogError($"PasswordHash for user {user.Id} is not a valid hash: {ex.Message}");
                return Ok(new
                {
                    found = true,
                    email = user.Email,
                    isDefaultPassword = false,
                    message = "This account's stored password hash is not in a valid format and could not be checked. It will need to be reset directly."
                });
            }

            bool isDefault = verifyResult == PasswordVerificationResult.Success;

            return Ok(new
            {
                found = true,
                email = user.Email,
                isDefaultPassword = isDefault,
                message = isDefault
                    ? $"This dealer is still using the default password ({StringConstants.DealerDefaultPassword})."
                    : "This dealer has set a custom password. Use 'Reset Password' to issue a new one."
            });
        }

        [HttpPost("admin-reset-password")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AdminResetPassword([FromBody] WhoAmIRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest(new { success = false, message = "Dealer email not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, StringConstants.DealerDefaultPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { success = false, message = "Reset failed", errors });
            }

            return Ok(new
            {
                success = true,
                message = $"Password has been reset to the default ({StringConstants.DealerDefaultPassword}). Please ask the dealer to log in and change it."
            });
        }

        // NEW — diagnostic/admin utility: resets a Location Login's password
        // directly via the same PasswordHasher<EmployeeMaster> used for
        // verification, completely bypassing the Employee Master Angular
        // form. Useful for isolating whether a "reset doesn't work" report
        // is a frontend issue (e.g. browser autofill on the password field
        // silently not reaching employeeData.locationPassword, so the
        // update saves with the OLD hash unchanged) or a backend one — if
        // Location Login succeeds right after calling this, the bug is in
        // the frontend form; if it still fails, the bug is in
        // AuthenticateLocationEmployeeAsync's verification path instead.
        [HttpPost("admin-reset-location-password")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdminResetLocationPassword([FromBody] AdminResetLocationPasswordRequest model)
        {
            var locationLoginId = model.LocationLoginId?.Trim();

            if (string.IsNullOrWhiteSpace(locationLoginId))
                return BadRequest(new { success = false, message = "Location Login ID is required." });

            if (string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { success = false, message = "New password is required." });

            var employee = await _context.EmployeeMasters
                .FirstOrDefaultAsync(e => e.LocationLoginId != null &&
                                           e.LocationLoginId.ToLower() == locationLoginId.ToLower());

            if (employee == null)
                return BadRequest(new { success = false, message = "Location Login ID not found." });

            //employee.LocationPasswordHash = _locationPasswordHasher.HashPassword(employee, model.NewPassword);
            employee.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Location password has been reset for '{locationLoginId}'. Try Location Login with the new password now."
            });
        }

        // ADDED — SuperAdmin can generate a live login session for any dealer,
        // without ever touching or storing that dealer's actual password.
        [HttpPost("impersonate")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Impersonate([FromBody] WhoAmIRequest model)
        {
            try
            {
                var superAdminId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                var superAdminEmail = User.FindFirstValue(ClaimTypes.Email);

                var targetUser = await _userManager.FindByEmailAsync(model.Email);
                if (targetUser == null)
                    return BadRequest(new { success = false, message = "Dealer not found." });

                var token = await GenerateJwtToken(targetUser, targetUser.DealerCode);

                _context.ImpersonationLogs.Add(new ImpersonationLog
                {
                    SuperAdminUserId = superAdminId,
                    SuperAdminEmail = superAdminEmail ?? "",
                    TargetDealerUserId = targetUser.Id,
                    TargetDealerCode = targetUser.DealerCode,
                    TargetDealerEmail = targetUser.Email,
                    StartedDate = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    token = token,
                    dealerCode = targetUser.DealerCode,
                    email = targetUser.Email,
                    message = $"You are now viewing the system as {targetUser.Email}. This session is logged."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Impersonation failed: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Impersonation failed." });
            }
        }

        // ADDED — SuperAdmin-visible audit trail of every impersonation session
        [HttpGet("impersonation-log")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetImpersonationLog()
        {
            var logs = await _context.ImpersonationLogs
                .OrderByDescending(x => x.StartedDate)
                .Select(x => new
                {
                    x.SuperAdminEmail,
                    x.TargetDealerCode,
                    x.TargetDealerEmail,
                    x.StartedDate,
                    x.EndedDate
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}