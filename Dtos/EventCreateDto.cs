namespace MyEventsApi.Dto
{
    public class EventCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        //public string Location { get; set; } = string.Empty;
    }
}
