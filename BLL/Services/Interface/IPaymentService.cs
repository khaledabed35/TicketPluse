using BLL.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto?> ProcessCheckoutAsync(CheckoutRequestDto checkoutDto);

        Task<bool> HandlePaymentWebhookAsync(string transactionId, int orderId, string status);
    }
}
