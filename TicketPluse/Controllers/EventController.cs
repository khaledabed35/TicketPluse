using BLL.Services.Interface;
using DAL.Specification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TicketPluse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var eventid = await _eventService.GetEventByIdAsync(id);
            if (id == null)
            {
                return NotFound(new { message = "this event not founf" });
            }
            return Ok(eventid);
        }
    }
}
