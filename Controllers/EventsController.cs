using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Models;
using MyEventsApi.Dto;
using MyEventsApi.Data;
using MyEventsApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyEventsApi.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var item = await _context.Events
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .ToListAsync(ct);

            return Ok(item);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var item = await _context.Events
                .AsNoTracking()
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date
            };

            _context.Events.Add(entity);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id, }, entity);
        }

        public class EventPatchDto
        {
            public string? Title { get; set; } 
            public string? Description { get; set; } 
            public DateTime? Date { get; set; }
        }


        [HttpPatch("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Patch(int id, [FromBody] EventPatchDto dto, CancellationToken ct)
        {
            var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null) return NotFound();

            if(dto.Title is not null) entity.Title = dto.Title;
            if (dto.Description is not null) entity.Description = dto.Description;
            if (dto.Date is not null) entity.Date = dto.Date.Value;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null) return NotFound();

            _context.Events.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        private Guid GetUserIdFromToken()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.Parse(sub!);
        }


        [HttpPost("{eventId:int}/join")]
        [Authorize] 
        public async Task<IActionResult> JoinEvent(int eventId, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();

            var ev = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId, ct);

            if (ev is null) return NotFound($"Event {eventId} not found");

            var already = await _context.Participants
                .AnyAsync(p => p.EventId == eventId && p.UserId == userId, ct);
            if (already) return Conflict("Already joined this event");



            _context.Participants.Add(new Participant
            {
                EventId = ev.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
            return Ok(new { message = "Joined", eventId});
        }


        [HttpPost("{eventId:int}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveEvent(int eventId, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, ct);

            if (participant is null) return NotFound("User is not a participant of this event");

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync(ct);

            return Ok(new { message = "Left", eventId, userId });
        }
    }
}