using Azure.Identity;
using janaez.webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace janaez.webapi.Controllers
{
    [Route("auth")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;

        }

        [BasicAuth]
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel register)
        {
            var user = new ApplicationUser
            {
                UserName = register.UserName,
                Email = register.Email,
                EmailConfirmed = true
            };

            //var result = await _userManager.CreateAsync(user, "Admin@123");
            var result = await _userManager.CreateAsync(user, register.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "User created successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel loginReq)
        {
            var result = await _signInManager.PasswordSignInAsync(
                loginReq.userLogin,
                loginReq.password,
                isPersistent: true,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Ok(new { message = "Logged in successfully" });
            }

            return Unauthorized(new { message = "Invalid username or password" });
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }


        [Authorize]
        [HttpGet("isAdmin")]
        public async Task<IActionResult> authorized()
        {
            return Ok(true);
        }

        [BasicAuth]
        [HttpPost("set-password")]
        public async Task<IActionResult> SetUserPassword([FromBody] SetPasswordDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Remove current password (if it exists)
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                    return BadRequest(removeResult.Errors);
            }

            // Add the new password
            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addResult.Succeeded)
                return BadRequest(addResult.Errors);

            return Ok(new { message = "Password updated successfully" });
        }
    }
}
