using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data;
using DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class BookinService : IBookkingService
    {
        private readonly IGenaricRePo<Ticket> _ticket;
        private readonly IGenaricRePo<Seat> _seat;
        private readonly IGenaricRePo<Order> _order;
        private readonly ICacheService _cacheService; // 🌟 حقن الـ Redis Cache Service

        public BookinService(
            IGenaricRePo<Order> orderRepo,
            IGenaricRePo<Seat> seatRepo,
            IGenaricRePo<Ticket> ticketRepo,
            ICacheService cacheService) // 🌟 ضفناها في الـ Constructor
        {
            _order = orderRepo;
            _seat = seatRepo;
            _ticket = ticketRepo;
            _cacheService = cacheService;
        }

        public async Task<OrderResponseDto?> BookSeatAsync(Guid userId, BookingRequestDto dto)
        {
            string lockKey = $"lock:seat:{dto.SeatId}";

            var isLocked = await _cacheService.GetAsync<string>(lockKey);
            if (isLocked != null)
            {
                return null;
            }

            await _cacheService.SetAsync(lockKey, userId.ToString(), TimeSpan.FromMinutes(1));

            try
            {
                var seat = await _seat.GetByIdAsync(dto.SeatId);

                if (seat == null || seat.Status != SeatStatus.Available)
                {
                    await _cacheService.RemoveAsync(lockKey);
                    return null;
                }

                seat.Status = SeatStatus.Locked;
                _seat.Update(seat);

                var order = new Order
                {
                    Uid = userId,
                    sid = seat.Id,
                    total_price = seat.Price,
                    PaymentStatus = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };

                await _order.AddAsync(order);
                await _order.savechange();
                await _seat.savechange();

                string eventSeatsCacheKey = $"event:{seat.EventId}:seats";
                await _cacheService.RemoveAsync(eventSeatsCacheKey);

                return new OrderResponseDto
                {
                    OrderId = order.Oid,
                    UserId = order.Uid,
                    SeatId = order.sid,
                    TotalPrice = order.total_price,
                    CreatedAt = order.CreatedAt,
                    ExpiresAt = order.ExpiresAt,
                    PaymentStatus = order.PaymentStatus.ToString()
                };
            }
            finally
            {
                
                await _cacheService.RemoveAsync(lockKey);
            }
        }

        public async Task<bool> CancelBookingAsync(int orderId)
        {
            var order = await _order.GetByIdAsync(orderId);
            if (order == null) return false;
            if (order.PaymentStatus == PaymentStatus.Paid) return false;

            var seat = await _seat.GetByIdAsync(order.sid);
            if (seat != null)
            {
                seat.Status = SeatStatus.Available;
                _seat.Update(seat);
            }

            order.PaymentStatus = PaymentStatus.Failed;
            _order.Update(order);
            await _seat.savechange();
            await _order.savechange();

            if (seat != null)
            {
                await _cacheService.RemoveAsync($"event:{seat.EventId}:seats");
            }

            return true;
        }

        public async Task<bool> ConfirmPaymentAsync(int orderId, string transactionId)
        {
            var order = await _order.GetByIdAsync(orderId);

            if (order == null || order.PaymentStatus != PaymentStatus.Pending)
                return false;

            var seat = await _seat.GetByIdAsync(order.sid);
            if (seat == null) return false;

            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentGatewayTransactionId = transactionId;
            _order.Update(order);

            seat.Status = SeatStatus.Sold;
            _seat.Update(seat);

            var ticket = new Ticket
            {
                Orderid = order.Oid,
                Seatid = seat.Id,
                IsUsed = false,
                TicketCode = "TKT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                TicketQR = "QR_" + Guid.NewGuid().ToString("N"),
                IssuedAt = DateTime.UtcNow
            };

            await _ticket.AddAsync(ticket);
            await _order.savechange();
            await _seat.savechange();

            // 🌟 5. الكرسي اتمباع رسميّاً (Sold)، امسح الكاش عشان يتشال تماماً من قائمة الكراسي المتاحة
            await _cacheService.RemoveAsync($"event:{seat.EventId}:seats");

            return true;
        }

        public async Task<IEnumerable<OrderResponseDto>> GetUserBookingsAsync(Guid userId)
        {
            // 💡 نصيحة للمستقبل: GetAllAsync وباقي الـ Linq بيحصل محلياً، يفضل قدام تعمل لها Specification مخصصة بـ UserId
            var allOrders = await _order.GetAllAsync();
            var userOrders = allOrders.Where(o => o.Uid == userId).ToList();

            return userOrders.Select(o => new OrderResponseDto
            {
                OrderId = o.Oid,
                UserId = o.Uid,
                SeatId = o.sid,
                TotalPrice = o.total_price,
                CreatedAt = o.CreatedAt,
                ExpiresAt = o.ExpiresAt,
                PaymentStatus = o.PaymentStatus.ToString(),
                TransactionId = o.PaymentGatewayTransactionId
            }).ToList();
        }
    }
}