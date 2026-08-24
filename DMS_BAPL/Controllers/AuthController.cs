using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.BgEmployeeMasterService;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.EmailService;
using DMS_BAPL_Data.Services.EmployeeMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace DMS_BAPL_Api.Controllers
{
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
        private readonly ILocationMasterService _locationMasterService;

        public AuthController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailService emailService,
            IConfiguration configuration, IWebHostEnvironment env,
            IDealerMasterService dealerMasterService, ILogger<AuthController> logger,
            IEmployeeService employeeService, IBgEmployeeMasterService bgEmployeeMasterService, ILocationMasterService locationMasterService)
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
                    // NEW — this identifier might be a Location Login ID rather than a
                    // regular email/username. Employees who only have a Location Login
                    // (no "Create Login Account" email/password) have no ApplicationUser
                    // at all, so FindByNameAsync/FindByEmailAsync above will always miss
                    // them. Checking this first — before dealer auto-provisioning — is
                    // what lets the single login form on the frontend work for them too,
                    // instead of always failing with "Username not found."
                    var locationAttempt = await TryLocationLoginAsync(normalizedUsername, model.Password, requestedLocationCode: null);

                    if (locationAttempt.Outcome != LocationLoginOutcome.NotFound)
                        return await BuildLocationLoginResponse(locationAttempt);

                    // Not a Location Login ID either — fall back to the existing
                    // dealer auto-provisioning behavior.
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
        /// Outcome of attempting to authenticate via the separate Location
        /// Login credential (EmployeeMaster.LocationLoginId / LocationPasswordHash).
        /// Shared between <see cref="Login"/> (which falls back to this when the
        /// identifier isn't a known ApplicationUser) and the dedicated
        /// <see cref="LocationLogin"/> endpoint, so both return identical
        /// responses for the same outcome.
        /// </summary>
        private enum LocationLoginOutcome
        {
            NotFound,
            InvalidPassword,
            Inactive,
            NoLocationsAssigned,
            RequiresSelection,
            Success
        }

        private record LocationLoginAttemptResult(
             LocationLoginOutcome Outcome,
             EmployeeMaster? Employee = null,
             List<string>? AvailableLocations = null,
             string? ResolvedLocationCode = null,
             List<string>? FunctionalRoles = null,
             string? LocationRoleId = null,
             string? LocationRoleName = null
          );

        /// <summary>
        /// Verifies a Location Login ID + password directly against
        /// EmployeeMaster (there's no ApplicationUser behind this credential),
        /// and resolves which of the employee's assigned locations
        /// (EmployeeMaster.LocationCode is a comma-separated list — see
        /// EmployeeMasterComponent.selectedLocations on the Angular side) the
        /// resulting session should be scoped to. Returns RequiresSelection
        /// instead of picking one when the employee has more than one assigned
        /// location and requestedLocationCode wasn't supplied or doesn't match.
        /// </summary>
        private async Task<LocationLoginAttemptResult> TryLocationLoginAsync(string locationLoginId, string password, string? requestedLocationCode)
        {
            var employee = await _employeeService.GetEmployeeByLocationLoginId(locationLoginId);

            if (employee == null || string.IsNullOrWhiteSpace(employee.LocationPasswordHash))
                return new LocationLoginAttemptResult(LocationLoginOutcome.NotFound);

            if (!employee.IsActive)
                return new LocationLoginAttemptResult(LocationLoginOutcome.Inactive, employee);

            var hasher = new PasswordHasher<EmployeeMaster>();
            var verification = hasher.VerifyHashedPassword(employee, employee.LocationPasswordHash, password);

            if (verification == PasswordVerificationResult.Failed)
                return new LocationLoginAttemptResult(LocationLoginOutcome.InvalidPassword, employee);

            var assignedLocations = (employee.LocationCode ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            if (assignedLocations.Count == 0)
                return new LocationLoginAttemptResult(LocationLoginOutcome.NoLocationsAssigned, employee);

            var resolvedLocationCode = requestedLocationCode?.Trim();

            if (string.IsNullOrWhiteSpace(resolvedLocationCode))
            {
                if (assignedLocations.Count > 1)
                    return new LocationLoginAttemptResult(LocationLoginOutcome.RequiresSelection, employee, assignedLocations);

                resolvedLocationCode = assignedLocations[0];
            }
            else if (!assignedLocations.Contains(resolvedLocationCode, StringComparer.OrdinalIgnoreCase))
            {
                return new LocationLoginAttemptResult(LocationLoginOutcome.RequiresSelection, employee, assignedLocations);
            }

            var mappings = await _employeeService.GetRoleMappings(employee.Id);
            var functionalRoleNames = mappings
                .Select(m => m.RoleName)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var (resolvedRoleId, resolvedRoleName) = await _locationMasterService
                .GetRoleByDealerAndLocationCodeAsync(employee.DealerCode, resolvedLocationCode);

            return new LocationLoginAttemptResult(
                LocationLoginOutcome.Success,
                employee,
                assignedLocations,
                resolvedLocationCode,
                functionalRoleNames,
                resolvedRoleId,
                resolvedRoleName
            );
        }

        /// <summary>
        /// Turns a LocationLoginAttemptResult into the actual HTTP
        /// response, minting the JWT on Success via GenerateLocationJwtToken.
        /// </summary>
        private async Task<IActionResult> BuildLocationLoginResponse(LocationLoginAttemptResult attempt)
        {
            switch (attempt.Outcome)
            {
                case LocationLoginOutcome.InvalidPassword:
                case LocationLoginOutcome.NotFound:
                    return Unauthorized(new { message = "Invalid Location Login ID or Password." });

                case LocationLoginOutcome.Inactive:
                    return Unauthorized(new { message = "This employee's account is inactive." });

                case LocationLoginOutcome.NoLocationsAssigned:
                    return Unauthorized(new { message = "No locations are assigned to this employee." });

                case LocationLoginOutcome.RequiresSelection:
                    return Ok(new
                    {
                        requiresLocationSelection = true,
                        availableLocations = attempt.AvailableLocations,
                        status = "selection_required",
                        message = "Select a location to continue."
                    });

                case LocationLoginOutcome.Success:
                default:
                    var employee = attempt.Employee!;
                    var dealerInfo = !string.IsNullOrWhiteSpace(employee.DealerCode)
                        ? await _dealerMasterService.GetDealerByCode(employee.DealerCode)
                        : null;

                    var token = GenerateLocationJwtToken(
                        employee,
                        attempt.ResolvedLocationCode!,
                        attempt.FunctionalRoles!,
                        attempt.LocationRoleId);

                    return Ok(new
                    {
                        employeeId = employee.Id,
                        employeeCode = employee.EmployeeCode,
                        firstName = employee.FirstName,
                        lastName = employee.LastName,
                        dealerCode = employee.DealerCode,
                        locationCode = attempt.ResolvedLocationCode,
                        locationRoleId = attempt.LocationRoleId,
                        locationRoleName = attempt.LocationRoleName,
                        token = token,
                        role = "Employee",
                        roles = attempt.FunctionalRoles,
                        compName = dealerInfo?.Compname,
                        status = "success",
                        message = "Login successful"
                    });
            }
        }

        /// <summary>
        /// Authenticates a dealer employee via the separate Location Login
        /// credential (EmployeeMaster.LocationLoginId / LocationPasswordHash),
        /// instead of the email/password Identity login <see cref="Login"/> uses.
        /// Kept as its own endpoint mainly for the location-selection round trip:
        /// when an employee has several assigned locations, call this again with
        /// LocationCode set to the one the user picked from
        /// requiresLocationSelection's availableLocations. For the common
        /// single-location case, <see cref="Login"/> already handles Location
        /// Login automatically as a fallback, so the existing single login form
        /// works for these employees without any frontend changes.
        /// </summary>
        [HttpPost("location-login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LocationLogin([FromBody] LocationLoginRequestDto model)
        {
            try
            {
                var locationLoginId = model?.LocationLoginId?.Trim();

                if (string.IsNullOrWhiteSpace(locationLoginId) || string.IsNullOrWhiteSpace(model?.Password))
                    return Unauthorized(new { message = "Location Login ID and Password are required." });

                var attempt = await TryLocationLoginAsync(locationLoginId, model.Password, model.LocationCode);

                return await BuildLocationLoginResponse(attempt);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during Location Login: {ex.Message}");
                throw;
            }
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
                new Claim(ClaimTypes.Name, dealerCode ?? string.Empty),  // dealer code, not email
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

        /// <summary>
        /// Generates a JWT for a Location Login session. There's no
        /// ApplicationUser behind this credential (it's verified directly
        /// against EmployeeMaster.LocationPasswordHash), so claims are built
        /// straight from the EmployeeMaster row instead of via UserManager.
        /// A "LocationCode" claim and a "LoginType": "Location" marker are
        /// added on top of what GenerateJwtToken emits, so anything reading
        /// the token can tell it's location-scoped and which location it's
        /// scoped to.
        /// </summary>
        private string GenerateLocationJwtToken(EmployeeMaster employee, string locationCode, IEnumerable<string> functionalRoles, string? locationRoleId)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
            {
                new Claim("DealerCode", employee.DealerCode ?? string.Empty),
                new Claim("LocationCode", locationCode ?? string.Empty),
                new Claim("EmployeeCode", employee.EmployeeCode ?? string.Empty),
                new Claim("LoginType", "Location"),
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.EmployeeCode ?? string.Empty),
                new Claim(ClaimTypes.Role, "Employee"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // FIX: this parameter was accepted but never actually turned into a
            // claim, so a Location Login token never carried "LocationRoleId" —
            // RoleWiseMenuRightService.GetMenuRightByRoleId and
            // DealerManagerController.GetMyAccess both read
            // User.FindFirst("LocationRoleId"), so neither could ever find the
            // location's assigned role, and forms granted to that location never
            // showed up after logging in from it. This is the actual fix.
            if (!string.IsNullOrWhiteSpace(locationRoleId))
            {
                claims.Add(new Claim("LocationRoleId", locationRoleId));
            }

            claims.AddRange(functionalRoles.Select(role => new Claim(ClaimTypes.Role, role)));

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

    }
}