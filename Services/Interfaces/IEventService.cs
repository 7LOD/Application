using MyEventsApi.Dtos;


namespace MyEventsApi.Services.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<EventResponseDto>> GetAllAsync(CancellationToken ct);
        Task<EventResponseDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<EventResponseDto> CreateEventAsync(EventCreateDto dto, Guid userId, CancellationToken ct);
        Task<bool> UpdateEventAsync(int id, EventUpdateDto dto, Guid userId, CancellationToken ct);
        Task<bool> DeleteEventAsync(int id, Guid userId, CancellationToken ct);
        Task<bool> JoinEventAsync(int eventId, Guid userId, CancellationToken ct);
        Task<bool> LeaveEventAsync(int eventId, Guid userId, CancellationToken ct);
        Task<IEnumerable<EventResponseDto>> GetEventInPeriodAsync(DateTime start, DateTime end, CancellationToken ct);
        Task<IEnumerable<EventResponseDto>> SearchEventAsync(string query, CancellationToken ct);
    }
}
