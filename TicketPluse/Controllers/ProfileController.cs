using BLL.Dto;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduMangment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User ID not found in token" });

            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound(new { Message = "Profile not found" });

            return Ok(profile);
        }

        [HttpGet("GetAllProfiles")]
        [Authorize(Roles = "Admin")] // متاح فقط لمن يحمل دور Admin
        public async Task<IActionResult> GetAllProfiles()
        {
            var profiles = await _profileService.GetProfilesAsync();
            return Ok(profiles);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _profileService.UpdateProfileAsync(userId, dto);
            if (!success)
                return BadRequest(new { Message = "Failed to update profile data" });

            return Ok(new { Message = "Profile updated successfully" });
        }
    }
}