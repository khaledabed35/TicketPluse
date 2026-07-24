namespace TicketPluse.Dto
{
    public class EventDto
    {
        public Guid Eid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Place { get; set; }
        public int TotalCapacity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
