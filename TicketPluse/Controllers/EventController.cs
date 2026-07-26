using BLL.Services.Interface;
using DAL.Specification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TicketPluse.Dto;

namespace EduMangment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("all-events")]
        public async Task<IActionResult> GetAllEvents([FromQuery] EventQueryParameters queryParams)
        {
            var events = await _eventService.GetAllEventsAsync(queryParams);
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null) return NotFound(new { Message = "Event not found" });
            return Ok(@event);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newEvent = await _eventService.CreateEventAsync(dto);
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Eid }, newEvent);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedEvent = await _eventService.UpdateEventAsync(id, dto);
            if (updatedEvent == null) return NotFound(new { Message = "Event not found" });

            return Ok(new { Message = "Event updated successfully", Event = updatedEvent });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var success = await _eventService.DeleteEventAsync(id);
            if (!success) return NotFound(new { Message = "Event not found" });

            return Ok(new { Message = "Event and its related seats deleted successfully" });
        }

        [HttpGet("{id}/seats")]
        public async Task<IActionResult> GetEventSeats(Guid id)
        {
            var seats = await _eventService.GetEventSeatsAsync(id);
            return Ok(seats);
        }
    }
}