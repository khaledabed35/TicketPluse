using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data;
using DAL.Repository.Interface;
using DAL.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class SeatServices : ISeatService
    {
        private readonly IGenaricRePo<Seat> _seat;
        private readonly IGenaricRePo<Event> _event;
        private readonly ICacheService _cacheService; // 🌟 حقن الـ Redis Cache

        public SeatServices(
            IGenaricRePo<Seat> seat,
            IGenaricRePo<Event> @event,
            ICacheService cacheService) // 🌟 ضفناها في الـ Constructor
        {
            _seat = seat;
            _event = @event;
            _cacheService = cacheService;
        }

        public async Task<string> CreateSeatAsync(SeatDto dto)
        {
            var eventExists = await _event.GetByIdAsync(dto.EventId);

            if (eventExists == null)
                return "Event not found";

            var spec = new SeatFilterSpecification(dto);
            var existingSeat = await _seat.GetWithSpecAsync(spec);

            if (existingSeat.Any())
                return "Seat already exists";

            var seat = new Seat
            {
                EventId = dto.EventId ?? Guid.Empty,
                Section = dto.Section,
                Row = dto.Row,
                Number = dto.Number,
                Price = dto.Price ?? 0m,
                Status = dto.Status ?? SeatStatus.Available
            };

            await _seat.AddAsync(seat);
            await _seat.savechange();

            // 🌟 طالما ضفنا كرسي جديد، طير كاش المقاعد المتاح للإيفينت ده
            string cacheKey = $"seats:event:{dto.EventId}";
            await _cacheService.RemoveAsync(cacheKey);

            return "Seat created successfully";
        }

        public async Task<bool> DeleteSeatAsync(Guid seatId)
        {
            var seat = await _seat.GetByIdAsync(seatId);

            if (seat == null)
                return false;

            _seat.Delete(seat);
            await _seat.savechange();

            // 🌟 طير كاش الإيفينت عشان الكرسي اللي اتمسح يختفي فوراً من عند اليوزرز
            string cacheKey = $"seats:event:{seat.EventId}";
            await _cacheService.RemoveAsync(cacheKey);

            return true;
        }

        public async Task<IEnumerable<SeatDto>> GetAllSeatAsync(SeatDto filter)
        {
            try
            {
                // 🌟 عمل مفتاح كاش فريد لكل EventId مبعوت في الفلتر
                // لو الـ EventId بـ null بنعمل كاش عام للكل
                string cacheKey = filter.EventId.HasValue
                    ? $"seats:event:{filter.EventId}"
                    : "seats:all";

                // 1. حاول تقرأ من الـ Redis أولاً
                var cachedSeats = await _cacheService.GetAsync<List<SeatDto>>(cacheKey);
                if (cachedSeats != null)
                {
                    return cachedSeats; // رجع الداتا طيران ✈️
                }

                // 2. لو مش متكشة، هاتها من الـ DB بالـ Specification بتاعتك
                var spec = new SeatFilterSpecification(filter);
                var seats = await _seat.GetWithSpecAsync(spec);

                var seatDtos = seats.Select(s => new SeatDto
                {
                    EventId = s.EventId,
                    Number = s.Number,
                    Section = s.Section,
                    Row = s.Row,
                    Price = s.Price,
                    Status = s.Status
                }).ToList();

                // 3. خزن الداتا في الـ Redis تعيش لمدة 10 دقائق مثلاً
                if (seatDtos.Any())
                {
                    await _cacheService.SetAsync(cacheKey, seatDtos, TimeSpan.FromMinutes(10));
                }

                return seatDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching seats: " + ex.Message, ex);
            }
        }

        public async Task<SeatDto> GetSeatAsyncById(Guid seatId)
        {
            try
            {
                var seat = await _seat.GetByIdAsync(seatId);

                if (seat == null) return null!;

                return new SeatDto
                {
                    EventId = seat.EventId,
                    Number = seat.Number,
                    Section = seat.Section,
                    Row = seat.Row,
                    Price = seat.Price,
                    Status = seat.Status
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching seat by ID: " + ex.Message, ex);
            }

        }

        public async Task<string> UpdateSeatAsync(Guid seatId, SeatDto dto)
        {
            var seat = await _seat.GetByIdAsync(seatId);

            if (seat == null)
                return "Seat not found";

            if (seat.Status == SeatStatus.Sold)
                return "Cannot update a sold seat";

            var eventExists = await _event.GetByIdAsync(dto.EventId);

            if (eventExists == null)
                return "Event not found";

            var spec = new SeatFilterSpecification(dto);
            var duplicateSeat = await _seat.GetWithSpecAsync(spec);

            if (duplicateSeat.Any(s => s.Id != seatId))
                return "Seat already exists";

            var oldEventId = seat.EventId;

            seat.EventId = dto.EventId ?? seat.EventId;
            seat.Section = dto.Section;
            seat.Row = dto.Row;
            seat.Number = dto.Number;
            seat.Price = dto.Price ?? seat.Price;
            seat.Status = dto.Status ?? seat.Status;

            _seat.Update(seat);
            await _seat.savechange();

            await _cacheService.RemoveAsync($"seats:event:{oldEventId}");
            if (dto.EventId.HasValue && dto.EventId != oldEventId)
            {
                await _cacheService.RemoveAsync($"seats:event:{dto.EventId}");
            }

            return "Seat updated successfully";
        }
    }
}