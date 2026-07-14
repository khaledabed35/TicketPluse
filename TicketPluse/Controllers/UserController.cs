using BLL.Dto;
using BLL.Services.Interface;
using DAL.Specification;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace TicketPluse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUser _user;

        public UserController(IUser user)
        {
            _user = user;
        }

        // 1. جلب كل المستخدمين بالـ DTO
        [HttpGet("getAll-users")]
        public async Task<IActionResult> GetAll([FromQuery] UserQueryParameters pram)
        {
            var users = await _user.GetAllUsersAsync(pram);
            return Ok(users);
        }

        // 2. جلب مستخدم بالـ ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var userDto = await _user.GetUserById(id);
            if (userDto == null)
            {
                return NotFound(new { Message = $"User with ID {id} was not found." });
            }
            return Ok(userDto);
        }

        // 3. حذف مستخدم
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _user.DeleteUser(id);
            if (!success)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }
            return Ok(new { Message = "User deleted successfully." });
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedUser = await _user.UpdateUser(userDto);
            if (updatedUser == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            return Ok(new { Message = "User updated successfully.", Data = updatedUser });
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdUser = await _user.AddUser(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
    }
}