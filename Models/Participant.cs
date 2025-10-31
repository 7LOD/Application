namespace MyEventsApi.Models
{
    public class Participant
    {
        public Guid UserId { get; set; }
        public int EventId { get; set; }

        public User? User { get; set; }
        public Event? Event { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}