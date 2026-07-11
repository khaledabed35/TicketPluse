namespace DAL.Data
{
    public class Ticket
    {
        public int Tid { get; set; }

        public int Orderid { get; set; } // int متوافق تماماً مع Oid في الـ Order
        public Order order { get; set; } = null!;

        public Guid Seatid { get; set; } // Guid متوافق تماماً مع Id في الـ Seat
        public Seat seat { get; set; } = null!;

        public bool IsUsed { get; set; } = false;
        public string TicketCode { get; set; } = string.Empty;
        public string TicketQR { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}