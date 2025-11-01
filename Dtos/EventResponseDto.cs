
namespace MyEventsApi.Dtos
{
    public class EventResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public int? Capacity { get; set; }
        public bool IsPublic { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }

        public List<string>? ParticipantName { get; set; }
    }

}
