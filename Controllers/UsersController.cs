using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyEventsApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private UsersController(ApplicationDbContext context)
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

            var events = _context.Participants
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.Event!.Id,
                    p.Event.Title,
                    p.Event.Description,
                    p.Event.Date,
                    p.JoinedAt
                })
                .OrderBy(x => x.Date)
                .ToListAsync(ct);
            return Ok(await events);

        }
    }
}
