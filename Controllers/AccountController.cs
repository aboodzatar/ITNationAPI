using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using iTeamAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace iTeamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                FullName = model.FullName
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return Ok(new { message = "User registered successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {
                    // Generate JWT Token
                    var token = GenerateJwtToken(user);
                    return Ok(new { Token = token });
                }
            }

            return Unauthorized(new { message = "Invalid login attempt" });
        }
        

        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync(); // Fetch users first

            var userWithRoles = new List<object>(); // Initialize a list to store users with roles

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Get roles for each user
                userWithRoles.Add(new
                {   
                    user.UserName,
                    user.FullName,
                    user.PhoneNumber,
                    Roles = roles
                });
            }

            return Ok(userWithRoles); // Return the result
        }





        [HttpPost("admin-change-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminChangeUserPassword([FromBody] AdminChangePasswordModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
                return NotFound("User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password changed successfully by admin");
        }


        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password changed successfully");
        }


        [HttpPost("promote-to-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteToAdmin([FromBody] PromoteUserModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
                return NotFound("User not found");

            var isInAdminRole = await _userManager.IsInRoleAsync(user, "Admin");
            var isInUserRole = await _userManager.IsInRoleAsync(user, "User");

            if (isInAdminRole)
            {
                // Remove from Admin, add to User
                var removeResult = await _userManager.RemoveFromRoleAsync(user, "Admin");
                var addResult = await _userManager.AddToRoleAsync(user, "User");

                if (!removeResult.Succeeded || !addResult.Succeeded)
                    return BadRequest("Failed to demote user");

                return Ok("User demoted to regular user successfully.");
            }
            else if (isInUserRole || !isInAdminRole)
            {
                // Remove any conflicting roles if needed, then promote
                if (isInUserRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, "User");
                }

                var result = await _userManager.AddToRoleAsync(user, "Admin");

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok("User promoted to admin successfully.");
            }
            else
            {
                return BadRequest("User is in an unexpected role state.");
            }
        }

        [HttpGet("users-count")]
        //[Authorize(Roles = "Admin")] // Optional: only Admins can access this
        public async Task<IActionResult> GetUsersCount()
        {
            var count = await _userManager.Users.CountAsync();
            return Ok(new { userCount = count });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            var profile = new UserProfileDto
            {
                FullName = user.FullName,
                Username = user.UserName,
                Major = user.Major,
                Skills = user.Skills,
                PhoneNumber = user.PhoneNumber,
            };

            return Ok(profile);
        }


        [HttpPost("update-info")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found");

            // Update standard properties
            if (!string.IsNullOrEmpty(model.FullName))
                user.FullName = model.FullName;

            if (!string.IsNullOrEmpty(model.Major))
                user.Major = model.Major;

            if (!string.IsNullOrEmpty(model.Skills))
                user.Skills = model.Skills;

            // Update phone number using UserManager
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    return BadRequest(new { message = "Failed to update phone number", errors = setPhoneResult.Errors });
                }
            }

            // Save other changes
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { message = "Profile updated successfully" });

            return BadRequest(new { message = "Failed to update profile", errors = result.Errors });
        }


        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var roles = await _userManager.GetRolesAsync(user); // Get user roles

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", user.FullName),
                new Claim("Username", user.UserName),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }
}

