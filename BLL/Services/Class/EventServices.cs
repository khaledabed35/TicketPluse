using BLL.Services.Interface;
using DAL.Data;
using DAL.Repository.Interface;
using DAL.Specification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketPluse.Dto;

namespace BLL.Services.Class
{
    public class EventService : IEventService
    {
        private readonly IGenaricRePo<Event> _eventRepo;
        private readonly IGenaricRePo<Seat> _seatRepo;
        private readonly INotificationService _notificationService;
        private readonly ICacheService _cacheService;
        private readonly INotificationQueue _notificationQueue; // ✅ متعرفة تمام هنا

        // 🌟 التصليح هنا: ضفنا INotificationQueue notificationQueue جوه الـ Parameters وعملنا له ربط
        public EventService(
            IGenaricRePo<Event> eventRepo,
            IGenaricRePo<Seat> seatRepo,
            INotificationService notificationService,
            ICacheService cacheService,
            INotificationQueue notificationQueue) // 👈 ضفناه هنا
        {
            _eventRepo = eventRepo;
            _seatRepo = seatRepo;
            _notificationService = notificationService;
            _cacheService = cacheService;
            _notificationQueue = notificationQueue; // 👈 ربطناه هنا
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync(EventQueryParameters queryParams)
        {
            string cacheKey = "events:all";

            var cachedEvents = await _cacheService.GetAsync<List<EventDto>>(cacheKey);
            if (cachedEvents != null)
            {
                return cachedEvents;
            }

            var events = await _eventRepo.GetAllAsync();
            var filteredEvents = events.AsQueryable();

            var eventDtos = filteredEvents.Select(e => new EventDto
            {
                Eid = e.Eid,
                Title = e.Title,
                Place = e.Place,
                Description = e.Description,
                TotalCapacity = e.TotalCapacity,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            }).ToList();

            if (eventDtos.Any())
            {
                await _cacheService.SetAsync(cacheKey, eventDtos, TimeSpan.FromMinutes(30));
            }

            return eventDtos;
        }

        public async Task<EventDto?> GetEventByIdAsync(Guid id)
        {
            string cacheKey = $"event:{id}";

            var cachedEvent = await _cacheService.GetAsync<EventDto>(cacheKey);
            if (cachedEvent != null)
            {
                return cachedEvent;
            }

            var e = await _eventRepo.GetByIdAsync(id);
            if (e == null) return null;

            var dto = new EventDto
            {
                Eid = e.Eid,
                Title = e.Title,
                Place = e.Place,
                Description = e.Description,
                TotalCapacity = e.TotalCapacity,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            };

            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

            return dto;
        }

        public async Task<EventDto> CreateEventAsync(EventDto dto)
        {
            var newEvent = new Event
            {
                Eid = Guid.NewGuid(),
                Title = dto.Title,
                Place = dto.Place,
                Description = dto.Description,
                TotalCapacity = dto.TotalCapacity,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            await _eventRepo.AddAsync(newEvent);
            await _eventRepo.savechange();

            await _cacheService.RemoveAsync("events:all");

            string notificationTitle = "🔥 New Event Released!";
            string notificationMessage = $"Hurry up! Book your seat now for '{newEvent.Title}' at {newEvent.Place}.";

            // 🚀 دلوقتي دي هتشتغل طيارة وبدون أي Null Reference!
            await _notificationQueue.QueueNotificationAsync(new NotificationMessage
            {
                Title = notificationTitle,
                Message = notificationMessage
            });

            dto.Eid = newEvent.Eid;
            return dto;
        }

        public async Task<EventDto?> UpdateEventAsync(Guid id, EventDto dto)
        {
            var existingEvent = await _eventRepo.GetByIdAsync(id);
            if (existingEvent == null) return null;

            existingEvent.Title = dto.Title;
            existingEvent.Place = dto.Place;
            existingEvent.Description = dto.Description;
            existingEvent.TotalCapacity = dto.TotalCapacity;
            existingEvent.StartDate = dto.StartDate;
            existingEvent.EndDate = dto.EndDate;

            _eventRepo.Update(existingEvent);
            await _eventRepo.savechange();

            await _cacheService.RemoveAsync("events:all");
            await _cacheService.RemoveAsync($"event:{id}");

            dto.Eid = existingEvent.Eid;
            return dto;
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            var existingEvent = await _eventRepo.GetByIdAsync(id);
            if (existingEvent == null) return false;

            _eventRepo.Delete(existingEvent);
            await _eventRepo.savechange();

            await _cacheService.RemoveAsync("events:all");
            await _cacheService.RemoveAsync($"event:{id}");
            await _cacheService.RemoveAsync($"seats:event:{id}");

            return true;
        }

        public async Task<IEnumerable<Seat>> GetEventSeatsAsync(Guid eventId)
        {
            string cacheKey = $"event:{eventId}:seats_raw";

            var cachedSeats = await _cacheService.GetAsync<List<Seat>>(cacheKey);
            if (cachedSeats != null) return cachedSeats;

            var allSeats = await _seatRepo.GetAllAsync();
            var eventSeats = allSeats.Where(s => s.EventId == eventId).ToList();

            if (eventSeats.Any())
            {
                await _cacheService.SetAsync(cacheKey, eventSeats, TimeSpan.FromMinutes(5));
            }

            return eventSeats;
        }
    }
}