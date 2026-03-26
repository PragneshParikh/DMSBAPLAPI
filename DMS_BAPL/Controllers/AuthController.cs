using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.EmailService;
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

        public AuthController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailService emailService,
            IConfiguration configuration, IWebHostEnvironment env,
            IDealerMasterService dealerMasterService, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _env = env;
            _dealerMasterService = dealerMasterService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user and returns JWT token along with user info.
        /// </summary>
        /// <param name="model">Login model containing username and password.</param>
        /// <returns>
        /// 200 OK if login succeeds,
        /// 401 Unauthorized if username not found or password invalid,
        /// 500 Internal Server Error if an exception occurs.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user == null)
                    return Unauthorized(new { message = "Username not found." });

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                if (!result.Succeeded)
                    return Unauthorized(new { message = "Invalid password." });

                var dealerInfo = await _dealerMasterService.GetDealerByCode(model.Username);

                var roles = await _userManager.GetRolesAsync(user);

                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var token = await GenerateJwtToken(user);

                return Ok(new
                {
                    userId = user.Id,
                    email = user.Email,
                    userName = user.UserName,
                    lastLoginDate = user.LastLoginDate,
                    token = token,
                    role = roles.FirstOrDefault(),
                    compName = dealerInfo.Compname,
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
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}