namespace MyEventsApi.Models
{
    public class UserEventView
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Guid OrganizerId { get; set; }
        public bool IsOrganizer { get; set; }  
        public DateTime? JoinedAt { get; set; }
    }
}
