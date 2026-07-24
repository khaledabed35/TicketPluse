using Azure.Messaging;
using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data.AuthModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduMangment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authServices;
        private readonly IEmailService _emailAsync;

        public AuthController(IAuthService authServices, IEmailService emailAsync)
        {
            _authServices = authServices;
            _emailAsync = emailAsync;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] registerModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authServices.RegisterAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.token))
            {
                Response.Cookies.Append("jwtToken", result.token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.expireon
                });
            }

            return Ok(new
            {
                Message = "Registration completed successfully. Please check your email to confirm your account.",
                Username = result.username,
                Email = result.email
            });
        }
        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] loginModel model)
        {
            var result = await _authServices.LoginAsync(model);

            if (!result.IsAuthenticated)
                return Unauthorized(result);

            var cookies = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("jwtToken", result.token, cookies);

            return Ok(new
            {
                Message = "Login successful.",
                UserName = result.username,
                Email = result.email,
                Roles = result.role
            });
        }

        // ================= GET ALL USERS (ADMIN ONLY) =================
        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var adminIdStr = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(adminIdStr)) return BadRequest(new { Message = "User Not Found" });

                // 👈 1. تحويل الـ string لـ Guid لتتوافق مع السيرفس
                var adminGuid = Guid.Parse(adminIdStr);
                var result = await _authServices.GetAllUserAsync(adminGuid);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ================= RemoveAdminRole =================
        [HttpPost("RemoveAdminRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAdminRole([FromBody] RemoveRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { Message = "Email is required" });

            var adminIdStr = User.FindFirst("uid")?.Value;
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized(new { Message = "Unauthorized" });

            // 👈 2. تحويل الـ string لـ Guid
            var adminGuid = Guid.Parse(adminIdStr);
            var result = await _authServices.RemoveAdminRoleAsync(adminGuid, dto.Email);

            if (result == "Admin role removed successfully")
                return Ok(new { Message = result });

            if (result == "Access denied")
                return Forbid();

            return BadRequest(new { Message = result });
        }

        // ================= ASSIGN ROLE =================
        [HttpPost("AddRoleToUser")]
        public async Task<IActionResult> AddRoleToUser([FromBody] AddRoleModel model)
        {
            try
            {
                // 🌟 قراءة الـ uid بأكثر من طريقة لضمان عدم حدوث تضارب في أسامي الـ Claims
                var adminIdStr = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value
                                 ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(adminIdStr))
                    return Unauthorized(new { Message = "Unauthorized: Token or UID claim not found." });

                if (model == null || string.IsNullOrWhiteSpace(model.email) || string.IsNullOrWhiteSpace(model.role))
                    return BadRequest(new { Message = "Email and Role are required." });

                var result = await _authServices.AddRoleAsync(model);

                if (result != "Role added successfully")
                    return BadRequest(new { Message = result });

                return Ok(new { Message = $"Role '{model.role}' assigned successfully to {model.email}." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        // ================= LOGOUT =================
        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwtToken");
            return Ok(new { Message = "Logout successful!" });
        }

        // ================= CONFIRM EMAIL =================
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest("UserId and Token are required.");

            // 👈 4. تحويل الـ string لـ Guid
            var userGuid = Guid.Parse(userId);
            var user = await _authServices.GetuserData(userGuid);

            if (user == null)
                return NotFound("User not found.");

            var result = await _authServices.ConfirmEmail(userGuid, token);

            if (result != "email is confirmed")
                return BadRequest(new { Message = result });

            return Ok("Email confirmed successfully.");
        }

        // ================= FORGET PASSWORD =================
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto confirm)
        {
            if (string.IsNullOrWhiteSpace(confirm.Email))
            {
                return BadRequest(new
                {
                    Message = "Email is required."
                });
            }

            var result = await _authServices.ForgotPassword(confirm.Email);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(new
                {
                    Message = result
                });
            }

            return Ok(new
            {
                Message = "Password reset link has been sent successfully."
            });
        }

        // ================= FORGET PASSWORD CONFIRM =================
        [HttpPost("ForgotPasswordConfirmation")]
        public async Task<IActionResult> ForgotPasswordConfirmation([FromBody] forgetpasswordconfirm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authServices.ConfirmPassword(model);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(new { Message = result });
            }

            return Ok(new { Message = "Password has been reset successfully." });
        }

        // ================= CHECK AUTH =================
        [HttpGet("CheckAuth")]
        [Authorize]
        public async Task<IActionResult> CheckAuth()
        {
            var userIdStr = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userIdStr))
            {
                userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            // 👈 5. تحويل لـ Guid للتشيك على اليوزر
            var userGuid = Guid.Parse(userIdStr);
            var isAuthenticated = await _authServices.IsUser(userGuid);

            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            return Ok(new { IsAuthenticated = true });
        }

        // ================= CHECK ADMIN =================
        [HttpGet("CheckAdmin")]
        [Authorize]
        public async Task<IActionResult> CheckAdmin()
        {
            var userIdStr = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            // 👈 6. تحويل لـ Guid للتشيك على رول الأدمن
            var userGuid = Guid.Parse(userIdStr);
            var isAdmin = await _authServices.IsAdmin(userGuid);

            if (!isAdmin)
                return Forbid();

            return Ok(new { IsAdmin = true });
        }
    }
}