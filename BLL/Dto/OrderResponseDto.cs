using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Dto
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public Guid UserId { get; set; }
        public Guid SeatId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
    }
}
