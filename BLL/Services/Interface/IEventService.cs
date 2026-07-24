using DAL.Data;
using DAL.Specification;
using TicketPluse.Dto;

namespace BLL.Services.Interface
{
  

    public interface IEventService
    {
        Task<IEnumerable<EventDto>> GetAllEventsAsync(EventQueryParameters queryParams);

        Task<EventDto?> GetEventByIdAsync(Guid id);
        Task<EventDto> CreateEventAsync(EventDto dto);
        Task<EventDto?> UpdateEventAsync(Guid id, EventDto dto);
        Task<bool> DeleteEventAsync(Guid id);

        Task<IEnumerable<Seat>> GetEventSeatsAsync(Guid eventId);

    }
}