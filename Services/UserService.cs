using MyEventsApi.Services.Interfaces;
using MyEventsApi.Data;
using MyEventsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MyEventsApi.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserEventView>> GetUserEventsAsync(Guid userId, CancellationToken ct)
        {
            var joinedEvents = _context.Participants
                .Where(p => p.UserId == userId)
                .Select(p => new UserEventView
                {
                    Id = p.Event!.Id,
                    Title = p.Event.Title,
                    Description = p.Event.Description,
                    Date = p.Event.Date,
                    OrganizerId = p.Event.OrganizerId,
                    IsOrganizer = false,
                    JoinedAt = p.JoinedAt
                });

            var createdEvents = _context.Events
                .Where(e => e.OrganizerId == userId)
                .Select(e => new UserEventView
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    OrganizerId = e.OrganizerId,
                    IsOrganizer = true,
                    JoinedAt = null
                });

            var allEvents = await joinedEvents
                .Union(createdEvents)
                .OrderBy(e => e.Date)
                .ToListAsync(ct);

            return allEvents;
        }

    }
}
