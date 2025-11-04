using MyEventsApi.Models;

namespace MyEventsApi.Services.Interfaces
{
    public interface IUserService
    {
        public Task<IEnumerable<UserEventView>> GetUserEventsAsync(Guid userId, CancellationToken ct);
    }
}
