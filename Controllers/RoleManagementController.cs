using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using iTeamAPI.Models;

namespace iTeamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]  // Only admins can access this
    public class RoleManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("assignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentModel model)
        {
            // Check if the user exists
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if the role exists
            var roleExist = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExist)
            {
                return BadRequest(new { message = "Role does not exist" });
            }

            // Assign the role to the user
            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (result.Succeeded)
            {
                return Ok(new { message = $"{model.RoleName} role assigned to {model.Username}" });
            }

            return BadRequest(new { message = "Failed to assign role" });
        }
    }
}
