using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data;
using DAL.Repository.Interface;


namespace BLL.Services.Class
{
    public class BookinService : IBookkingService
    {
        private readonly IGenaricRePo<Ticket> _ticket;
        private readonly IGenaricRePo<Seat> _seat;
        private readonly IGenaricRePo<Order> _order;
        public BookinService (IGenaricRePo<Order> orderRepo,
            IGenaricRePo<Seat> seatRepo,
            IGenaricRePo<Ticket> ticketRepo)
        
            
       
        {
            _order = orderRepo;
            _seat = seatRepo;
            _ticket = ticketRepo;
        }
        public async Task<OrderResponseDto?> BookSeatAsync(Guid userId, BookingRequestDto dto)
        {
            // 1. التعديل هنا: بندور بالـ SeatId اللي جاي من الـ Dto مش الـ userId
            var seat = await _seat.GetByIdAsync(dto.SeatId);

            if (seat == null || seat.Status != SeatStatus.Available)
            {
                return null; // هيرجع null لو الكرسي مش موجود أو محجوز
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
            _seat.savechange();
            _order.savechange();
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
            return true;
        }

        public async Task<IEnumerable<OrderResponseDto>> GetUserBookingsAsync(Guid userId)
        {
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
