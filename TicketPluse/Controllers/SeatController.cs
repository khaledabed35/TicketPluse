using BLL.Dto;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace TicketPluse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatController :ControllerBase
    {
        private readonly ISeatService _seat;

        public SeatController(ISeatService seat)
        {
            _seat = seat;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSeat([FromQuery] SeatDto dto)
        {
            var seats = await _seat.GetAllSeatAsync(dto);

            if (!seats.Any())
                return NotFound(new { Message = "No matching seats found" });

            return Ok(seats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSeatById(Guid id)
        {
            var seat = await _seat.GetSeatAsyncById(id);

            if (seat == null)
                return NotFound(new { Message = "Seat not found" });

            return Ok(seat);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SeatDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _seat.CreateSeatAsync(dto);

            if (result == "Seat created successfully")
                return Ok(new { Message = result });

            return BadRequest(new { Message = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SeatDto dto)
        {
            var result = await _seat.UpdateSeatAsync(id, dto);

            if (result == "Seat updated successfully")
                return Ok(new { Message = result });

            if (result == "Seat not found")
                return NotFound(new { Message = result });

            return BadRequest(new { Message = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _seat.DeleteSeatAsync(id);

            if (!deleted)
                return NotFound(new { Message = "Seat not found" });

            return Ok(new { Message = "Seat deleted successfully" });
        }
    }
}