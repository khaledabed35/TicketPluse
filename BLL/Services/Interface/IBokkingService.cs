using BLL.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IBookkingService
    {
        Task<OrderResponseDto?> BookSeatAsync(Guid userId, BookingRequestDto dto);

        Task<bool> ConfirmPaymentAsync(int orderId, string transactionId);

        Task<IEnumerable<OrderResponseDto>> GetUserBookingsAsync(Guid userId);

        Task<bool> CancelBookingAsync(int orderId);
    }
}
