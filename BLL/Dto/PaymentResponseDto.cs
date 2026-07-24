using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Dto
{
    public class PaymentResponseDto
    {
        public int OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }
}
