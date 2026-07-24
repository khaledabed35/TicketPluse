using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data;
using DAL.Repository.Interface;
using DAL.Specification;

namespace BLL.Services.Class
{
    public class SeatServices : ISeatService
    {
        private readonly IGenaricRePo<Seat> _seat;
        private readonly IGenaricRePo<Event> _event;

        public SeatServices(
            IGenaricRePo<Seat> seat,
            IGenaricRePo<Event> @event)
        {
            _seat = seat;
            _event = @event;
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
                Price = dto.Price ?? 0m, // لو بـ null هينزل بـ 0
                Status = dto.Status ?? SeatStatus.Available
            };

            await _seat.AddAsync(seat);

            await _seat.savechange();

            return "Seat created successfully";
        }

        public async Task<bool> DeleteSeatAsync(Guid seatId)
        {
            var seat = await _seat.GetByIdAsync(seatId);

            if (seat == null)
                return false;

            _seat.Delete(seat);

            await _seat.savechange();

            return true;
        }

        public async Task<IEnumerable<SeatDto>> GetAllSeatAsync(SeatDto filter)
        {
            try
            {
                var spec = new SeatFilterSpecification(filter);

                var seats = await _seat.GetWithSpecAsync(spec);

                return seats.Select(s => new SeatDto
                {
                    EventId = s.EventId,
                    Number = s.Number,
                    Section = s.Section,
                    Row = s.Row,
                    Price = s.Price,
                    Status = s.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"بص يا هندسة الإيرور هنا أهو: {ex.Message} -> {ex.InnerException?.Message}", ex);
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
                throw new Exception($"بص يا هندسة الإيرور هنا أهو: {ex.Message} -> {ex.InnerException?.Message}");
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
            seat.EventId = dto.EventId ?? seat.EventId;
            seat.Section = dto.Section;
            seat.Row = dto.Row;
            seat.Number = dto.Number;
            seat.Price = dto.Price ?? seat.Price;
            seat.Status = dto.Status ?? seat.Status;

            _seat.Update(seat);

            await _seat.savechange();

            return "Seat updated successfully";
        }
    }
}