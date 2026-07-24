using BLL.Dto;

namespace BLL.Services.Interface
{
    public interface ISeatService
    {
        Task<IEnumerable<SeatDto>> GetAllSeatAsync(SeatDto filter);

        Task<SeatDto?> GetSeatAsyncById(Guid seatId);

        Task<string> CreateSeatAsync(SeatDto dto);

        Task<string> UpdateSeatAsync(Guid seatId, SeatDto dto);

        Task<bool> DeleteSeatAsync(Guid seatId);
    }
}
