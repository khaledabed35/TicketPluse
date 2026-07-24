using DAL.Data.AuthModel;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace DAL.Data
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Expired
    }
    public class Order
    {
        public int Oid { get; set; }
        public Guid Uid { get; set; }
        public App_user User { get; set; } = null!;


        public Guid sid { get; set; }

        public Seat Seat { get; set; } = null!;
        public decimal total_price { get; set; }
       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public string? PaymentGatewayTransactionId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
      
        public Ticket? Ticket { get; set; }
    }
}
