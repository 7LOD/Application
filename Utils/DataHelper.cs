using MyEventsApi.Dtos;
using MyEventsApi.Models;

namespace MyEventsApi.Utils
{
    public static class DataHelper
    {
        public static EventResponseDto MapToDto(Event e) => new EventResponseDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Date = e.Date,
            Location = e.Location,
            Capacity = e.Capacity,
            IsPublic = e.IsPublic,
            OrganizerName = e.Organizer?.DisplayName ?? "Unknown",
            ParticipantCount = e.Participants?.Count ?? 0
        };
        public static DateTime ToUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }
    }
}
