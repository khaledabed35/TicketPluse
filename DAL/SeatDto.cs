using DAL.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Dto
{
    public class SeatDto
    {
        public  string? Number { get; set; } // e.g., "A-12"
        public  string? Section { get; set; } // e.g., "VIP", "Category 1"
        public  string? Row { get; set; } // e.g., "Row 5"
        public decimal? Price { get; set; }
        public SeatStatus? Status { get; set; } = SeatStatus.Available;
        public Guid? EventId { get; set; }
    }
}
