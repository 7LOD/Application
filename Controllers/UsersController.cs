using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Data;
using MyEventsApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyEventsApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("me/events")]
        [Authorize]
        public async Task<IActionResult> GetMyEvents(CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userId = Guid.Parse(userIdClaim!);


            var joinedEvents =  _context.Participants
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

            var myEvents =  _context.Events
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
                .Union(myEvents)
                .OrderBy(e => e.Date)
                .ToListAsync(ct);

            return Ok(allEvents);

        }
    }
}
