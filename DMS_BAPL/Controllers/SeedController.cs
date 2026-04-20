using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/seed")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("roles")]
        public async Task<IActionResult> SeedRoles()
        {
            await RoleSeeder.SeedRoles(_roleManager);
            return Ok(new { message = "Roles seeded successfully" });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUser createUser)
        {
            var user = new IdentityUser
            {
                UserName = createUser.EmailId,
                Email = createUser.EmailId,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, createUser.Password);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    message = "User created successfully",
                    userId = user.Id
                });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(AssignRoleRequest assignRoleRequest)
        {
            var user = await _userManager.FindByIdAsync(assignRoleRequest.UserId);

            if (user == null)
                return BadRequest("User not found");

            await _userManager.AddToRoleAsync(user, assignRoleRequest.RoleName);

            return Ok("Role assigned successfully");
        }
    }
}

public class AssignRoleRequest
{
    public string UserId { get; set; }
    public string RoleName { get; set; }
}

public class CreateUser
{
    public string EmailId { get; set; }
    public string Password { get; set; }
}