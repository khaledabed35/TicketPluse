using BLL.Dto;
using BLL.Services.Class;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TicketPluse.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookkingService _bookingService;

        public BookingController(IBookkingService bookingService)
        {
            _bookingService = bookingService;
        }
        [HttpPost("book")]
        public async Task<IActionResult> BookSeat([FromBody] BookingRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 👈 الكود ده دلوقتي هيقرأ الـ ID تلقائياً من الـ Cookie اللي مبعوتة
            var userIdStr = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userGuid = Guid.Parse(userIdStr);
            var order = await _bookingService.BookSeatAsync(userGuid, dto);

            if (order == null)
            {
                return BadRequest(new { Message = "Seat is either unavailable, locked, or already sold." });
            }

            return Ok(order);
        }
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(int orderId, string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId)) return BadRequest("Transaction ID is required.");

            var success = await _bookingService.ConfirmPaymentAsync(orderId, transactionId);
            if (!success)
            {
                return BadRequest(new { Message = "Payment confirmation failed. Order may have expired or is already paid." });
            }

            return Ok(new { Message = "Payment confirmed successfully. Your ticket has been generated." });
        }

        // 3. جلب حجوزات المستخدم الحالي
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdStr = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userGuid = Guid.Parse(userIdStr);
            var bookings = await _bookingService.GetUserBookingsAsync(userGuid);

            return Ok(bookings);
        }

        // 4. إلغاء الحجز يدوياً قبل انتهاء الصلاحية
        [HttpDelete("cancel/{orderId}")]
        public async Task<IActionResult> CancelBooking(int orderId)
        {
            var success = await _bookingService.CancelBookingAsync(orderId);
            if (!success)
            {
                return BadRequest(new { Message = "Cannot cancel this order. It may not exist or is already paid." });
            }

            return Ok(new { Message = "Booking canceled successfully and seat is now available." });
        }
    }
}