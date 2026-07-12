using BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketPluse.Dto;

namespace TicketPluse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

       
        [HttpPost("book-seat")]
        public async Task<IActionResult> BookSeat(BookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _bookingService.BookSeatAsync(
                userId,
                dto.EventId,
                dto.SeatId);

            if (!result)
                return BadRequest(new
                {
                    Message = "Booking failed."
                });

            return Ok(new
            {
                Success = true,
                Message = "Seat booked successfully. Please complete the payment within 10 minutes."
            });
        }
    }
}