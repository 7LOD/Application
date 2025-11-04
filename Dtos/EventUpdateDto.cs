namespace MyEventsApi.Dtos
{
    public class EventUpdateDto
    {
        public string? Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime? Date { get; set; }

        public string? Location { get; set; } = string.Empty;

        public int? Capacity { get; set; }
    }
}
