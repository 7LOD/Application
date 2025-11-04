using Microsoft.EntityFrameworkCore;
using MyEventsApi.Data;
using MyEventsApi.Dtos;
using MyEventsApi.Services.Interfaces;
using MyEventsApi.Utils;
using MyEventsApi.Models;
using System.ComponentModel;
using System.Globalization;


namespace MyEventsApi.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventResponseDto>> GetAllAsync(CancellationToken ct)
        {
            var events = await _context.Events
                .AsNoTracking()
                .Where(e => e.IsPublic)
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                .OrderBy(e => e.Date)
                .ToListAsync(ct);


            return events.Select(DataHelper.MapToDto);

        }

        public async Task<EventResponseDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var ev = await _context.Events
                .AsNoTracking()
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            return ev is null ? null : DataHelper.MapToDto(ev);
        }

        public async Task<EventResponseDto> CreateEventAsync(EventCreateDto dto, Guid userId, CancellationToken ct)
        {
            var entity = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = DataHelper.ToUtc(dto.Date),
                Location = dto.Location,
                Capacity = dto.Capacity,
                IsPublic = dto.IsPublic,
                OrganizerId = userId
            };

            _context.Events.Add(entity);

            await _context.SaveChangesAsync(ct);

            return DataHelper.MapToDto(entity);
        }

        public async Task<bool> UpdateEventAsync(int id,  EventUpdateDto dto, Guid userId, CancellationToken ct)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

            if (ev is null || ev.OrganizerId != userId) return false;

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            if (dto.Date.HasValue)
            {
                ev.Date = DataHelper.ToUtc(dto.Date.Value);
            }
            ev.Location = dto.Location;

            ev.Capacity = dto.Capacity;
            
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteEventAsync(int id, Guid userId, CancellationToken ct)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

            if (ev is null || ev.OrganizerId != userId) return false;

            _context.Events.Remove(ev);

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> JoinEventAsync(int eventId, Guid userId, CancellationToken ct)
        {
            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId, ct);

            if (ev is null || ev.OrganizerId == userId) return false;

            if (ev.Capacity.HasValue && ev.Capacity.Value > 0 && ev.Participants.Count >= ev.Capacity.Value) return false;

            if (await _context.Participants.AnyAsync(p => p.EventId == eventId && p.UserId == userId, ct)) return false;

            _context.Participants.Add(new Participant { EventId = eventId, UserId = userId, JoinedAt = DateTime.UtcNow  });
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> LeaveEventAsync(int eventId, Guid userId, CancellationToken ct)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == userId, ct);
            if (participant is null) return false;

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<EventResponseDto>> GetEventInPeriodAsync(DateTime start, DateTime end, CancellationToken ct)
        {
            start = DataHelper.ToUtc(start);
            end = DataHelper.ToUtc(end).AddDays(1);

            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                .Where(e => e.IsPublic && e.Date >= start && e.Date <= end)
                .OrderBy(e => e.Date)
                .ToListAsync(ct);

            return events.Select(DataHelper.MapToDto);
        }

        public async Task<IEnumerable<EventResponseDto>> SearchEventAsync(string query, CancellationToken ct)
        {
            DateTime? parsedDate = null;
            if (DateTime.TryParseExact(
                query,
                new[] { "dd.MM.yyyy", "yyyy-MM-dd", "dd/MM/yyyy" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var d))
            {
                parsedDate = DataHelper.ToUtc(d);
            }

            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                .Where(e => e.IsPublic &&
                (EF.Functions.ILike(e.Title ?? "", $"%{query}%")
                || EF.Functions.ILike(e.Description ?? "", $"%{query}%")
                || EF.Functions.ILike(e.Location ?? "", $"%{query}%")
                || (parsedDate != null && e.Date.Date == parsedDate.Value.Date)))
                .OrderBy(e => e.Date)
                .ToListAsync(ct);

            return events.Select(DataHelper.MapToDto);
        }
    }
}
