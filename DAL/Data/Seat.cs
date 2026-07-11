using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Data
{
    public enum SeatStatus
    {
        Available,
        Locked,
        Sold
    }
    public class Seat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; } // Foreign Key
        public required string Number { get; set; } // e.g., "A-12"
        public required string Section { get; set; } // e.g., "VIP", "Category 1"
        public required string Row { get; set; } // e.g., "Row 5"
        public decimal Price { get; set; }
        public SeatStatus Status { get; set; } = SeatStatus.Available;

        // Navigation Properties
        public Event Event { get; set; } = null!;
    }
}
