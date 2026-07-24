using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data;
using DAL.Repository.Class;
using DAL.Repository.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Class
{
    public class PaymentService : IPaymentService
    {
        private readonly IGenaricRePo<DAL.Data.Order> _order;

        private readonly IBookkingService _bookingService; 
        public PaymentService(IGenaricRePo<DAL.Data.Order> orderRepo, IBookkingService bookingService)
        {
            _order = orderRepo;
            _bookingService = bookingService;
        }
        public async Task<PaymentResponseDto?> ProcessCheckoutAsync(CheckoutRequestDto checkoutDto)
        {
            var order = await _order.GetByIdAsync(checkoutDto.OrderId);

            if (order == null || order.PaymentStatus != PaymentStatus.Pending || DateTime.UtcNow > order.ExpiresAt)
            {
                return null;
            }

            string mockTransactionId = "TXN-" + Guid.NewGuid().ToString().Substring(0, 12).ToUpper();
            order.PaymentGatewayTransactionId = mockTransactionId;
            _order.Update(order);
            string paymentUrl = $"https://api.ticketpluse.com/payment/simulate?orderId={order.Oid}&amount={order.total_price}&txnId={mockTransactionId}";

            return new PaymentResponseDto
            {
                OrderId = order.Oid,
                TransactionId = mockTransactionId,
                AmountPaid = order.total_price,
                Status = "RedirectingToGateway",
                PaymentUrl = paymentUrl,
                PaymentDate = DateTime.UtcNow
            };
        }

        public async Task<bool> HandlePaymentWebhookAsync(string transactionId, int orderId, string status)
        {
            if (status.ToLower() == "success" || status.ToLower() == "paid")
            {
                var isConfirmed = await _bookingService.ConfirmPaymentAsync(orderId, transactionId);
                return isConfirmed;
            }

            if (status.ToLower() == "failed")
            {
                await _bookingService.CancelBookingAsync(orderId);
            }

            return false;
        }
    }
}

