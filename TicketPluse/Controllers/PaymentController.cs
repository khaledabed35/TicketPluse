using BLL.Dto;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TicketPluse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // 1. إنشاء رابط الدفع / Checkout لطلب حجز معين
        [Authorize] // يحتاج تسجيل دخول
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _paymentService.ProcessCheckoutAsync(dto);
            if (result == null)
            {
                return BadRequest(new { Message = "Cannot process payment. Order might be expired, paid, or not found." });
            }

            return Ok(result);
        }

        // 2. الـ Webhook (محاكاة استقبال إشعار نجاح الدفع من بوابة الدفع Stripe/Paymob)
        [HttpPost("webhook")]
        public async Task<IActionResult> PaymentWebhook([FromQuery] string transactionId, [FromQuery] int orderId, [FromQuery] string status)
        {
            if (string.IsNullOrEmpty(transactionId) || orderId <= 0 || string.IsNullOrEmpty(status))
            {
                return BadRequest(new { Message = "Invalid webhook parameters." });
            }

            var paymentConfirmed = await _paymentService.HandlePaymentWebhookAsync(transactionId, orderId, status);
            if (!paymentConfirmed)
            {
                return BadRequest(new { Message = "Failed to process payment callback. Order may already be processed or expired." });
            }

            return Ok(new { Message = "Webhook processed successfully. Order is paid and ticket is generated!" });
        }
    }
}