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
                .Where(e => e.IsPublic)
                .Include(e => e.Organizer)
                .OrderBy(e => e.Date)
                .Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    Capacity = e.Capacity,
                    IsPublic = e.IsPublic,
                    OrganizerName = e.Organizer!.DisplayName,
                    ParticipantCount = e.Participants.Count
                })
                .ToListAsync(ct);
                
                
            

            return Ok(item);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var item = await _context.Events
                .AsNoTracking()
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                .ThenInclude(p => p.User)
                .Where(e => e.Id == id)
                .Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    Capacity = e.Capacity,
                    IsPublic = e.IsPublic,
                    OrganizerName = e.Organizer!.DisplayName,
                    ParticipantCount = e.Participants.Count,
                    ParticipantName = e.Participants
                        .Select(p => p.User!.DisplayName)
                        .ToList()
                })
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userID = GetUserIdFromToken();

            var entity = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date,
                OrganizerId = userID,
                Location = dto.Location,
                Capacity = dto.Capacity == 0 ? null : dto.Capacity,
                IsPublic = dto.IsPublic,    
            };

            _context.Events.Add(entity);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id, }, MapToDto(entity));
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

            var userId = GetUserIdFromToken();
            if (entity.OrganizerId != userId)
            {
                return Forbid("You are not the organizer of this event");
            }


            if (dto.Title is not null) entity.Title = dto.Title;
            if (dto.Description is not null) entity.Description = dto.Description;
            if (dto.Date is not null)
            {
                if (dto.Date.Value <= DateTime.UtcNow)
                {
                    return BadRequest("Event cannot be in the past");
                }
                entity.Date = dto.Date.Value;
            }
         

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null) return NotFound();

            var userId = GetUserIdFromToken();
            if (entity.OrganizerId != userId)
            {
                return Forbid("You are not the organizer of this event");
            }


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


            if (ev.OrganizerId == userId)
            {
                return Conflict("Organizer cannot join their own event as participant");
            }
            if (ev.Capacity.HasValue)
            {
                var currectCount = await _context.Participants
                    .CountAsync(p => p.EventId == eventId, ct);
                if (currectCount >= ev.Capacity.Value)
                {
                    return BadRequest("Event is full");
                }
            }

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
        private static EventResponseDto MapToDto(Event e) => new EventResponseDto
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
    }
}