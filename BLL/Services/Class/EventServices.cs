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
        private readonly INotificationService _notificationService; // 👈 1. تعريف السيرفيس هنا

        // 👈 2. احقن الـ INotificationService جوة الكنستركتور هنا
        public EventService(IGenaricRePo<Event> eventRepo, IGenaricRePo<Seat> seatRepo, INotificationService notificationService)
        {
            _eventRepo = eventRepo;
            _seatRepo = seatRepo;
            _notificationService = notificationService; // 👈 ربط السيرفيس
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync(EventQueryParameters queryParams)
        {
            var events = await _eventRepo.GetAllAsync();
            var filteredEvents = events.AsQueryable();

            return filteredEvents.Select(e => new EventDto
            {
                Eid = e.Eid,
                Title = e.Title,
                Place = e.Place,
                Description = e.Description,
                TotalCapacity = e.TotalCapacity,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            }).ToList();
        }

        public async Task<EventDto?> GetEventByIdAsync(Guid id)
        {
            var e = await _eventRepo.GetByIdAsync(id);
            if (e == null) return null;

            return new EventDto
            {
                Eid = e.Eid,
                Title = e.Title,
                Place = e.Place,
                Description = e.Description,
                TotalCapacity = e.TotalCapacity,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            };
        }

        // 3. إنشاء إيفنت جديد مع الإشعارات
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

            // أ. حفظ الإيفنت أولاً في الداتابيز
            await _eventRepo.AddAsync(newEvent);
            await _eventRepo.savechange();

            // 🚀 ب. السحر هنا: إرسال الإشعار لجميع المستخدمين فوراً بعد الحفظ
            string notificationTitle = "🔥 New Event Released!";
            string notificationMessage = $"Hurry up! Book your seat now for '{newEvent.Title}' at {newEvent.Place}.";

            // 👈 الـ await هنا قاتلة وإلزامية عشان تستنى الحفظ يخلص
            await _notificationService.BroadcastNotificationAsync(notificationTitle, notificationMessage);

            dto.Eid = newEvent.Eid;
            return dto;
        }

        // 4. تعديل إيفنت موجود
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
            await _eventRepo.savechange(); // تأكد إن الـ savechange معمولة هنا برضه

            dto.Eid = existingEvent.Eid;
            return dto;
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            var existingEvent = await _eventRepo.GetByIdAsync(id);
            if (existingEvent == null) return false;

            _eventRepo.Delete(existingEvent);
            await _eventRepo.savechange();
            return true;
        }

        public async Task<IEnumerable<Seat>> GetEventSeatsAsync(Guid eventId)
        {
            var allSeats = await _seatRepo.GetAllAsync();
            return allSeats.Where(s => s.EventId == eventId).ToList();
        }
    }
}