namespace MyEventsApi.Models
{
    public class Event
    {
        public Guid OrganizerId { get; set; } 
        public User? Organizer { get; set; }
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }

        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}